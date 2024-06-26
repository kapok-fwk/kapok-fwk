﻿using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Entity;
using Microsoft.Extensions.DependencyInjection;
using Res = Kapok.View.Resources.Data.DataSetView;

namespace Kapok.View;

public class DataSetView<TEntry> : BindableObjectBase, IDataSetView<TEntry>
    where TEntry : class, new()
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IDataDomainScope DataDomainScope;
    protected readonly IEntityService<TEntry> EntityService;
    protected readonly ObservableCollection<TEntry> Collection = new();
    private bool _insertAllowed = true;
    private bool _modifyAllowed = true;
    private bool _deleteAllowed = true;
    private bool _canUserSort = true;
    private TEntry? _newEntry;
    private bool _isNewEntry;
    private TEntry? _current;
    private IList? _selectedEntries;
    private SortDirection _sortDirection = SortDirection.Ascending;
    private bool _isFilterVisible;
    private bool _isLoaded;
    private readonly List<TEntry> _entriesWithValidationErrors = new();
    private readonly List<DeferRefreshHolder> _deferredRefreshes = new();
    private bool _refreshDeferred;
    private bool _refreshPropertyLookupsDeferred;
    private bool _refreshPropertyLookupsOnlyOnEntryDeferred;
    private PropertyInfo[]? _sortBy;

    public DataSetView(IServiceProvider serviceProvider, IDataDomainScope dataDomainScope, IEntityService<TEntry>? entityService = null)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(dataDomainScope, nameof(dataDomainScope));

        ServiceProvider = serviceProvider;
        DataDomainScope = dataDomainScope;

        if (entityService == null)
        {
            entityService = dataDomainScope.GetEntityService<TEntry>();
            if (entityService == null)
            {
                throw new ArgumentException(string.Format(Res.ParameterEntityServiceNotDetermined, nameof(DataSetView<TEntry>)), nameof(entityService));
            }
        }
        EntityService = entityService;

        // TODO: this logic doesn't sound right and we might need to change this in the future
        IsDeferredSaveActive = typeof(IEntityDeferredCommitService).IsAssignableFrom(EntityService.GetType());

        Filter = new FilterSet<TEntry>();
        Filter.FilterChanged += Filter_FilterChanged;

        CollectionSubscribeEvents();

        SortBy = EntityService.Model.PrimaryKeyProperties;
        SortAscendingAction = new UIAction("SortAscending", SortAscending, CanSortAscending) { Image = "sort-az" };
        SortDescendingAction = new UIAction("SortDescending", SortDescending, CanSortDescending) { Image = "sort-za" };

        if (EntityService.IsReadOnly)
        {
            _insertAllowed = false;
            _modifyAllowed = false;
            _deleteAllowed = false;
        }

        Columns = new PropertyViewCollection<TEntry>(ServiceProvider, EntityService.Model, this);
        Columns.CollectionChanged += Columns_CollectionChanged;

        CreateNewEntryAction = new UIAction("CreateNewEntry", CreateNewEntry, CanCreateNewEntry) {Image = "table-row-new"};
        DeleteEntryAction = new UIDataSetSelectionAction<TEntry>("DeleteEntry", DeleteEntry, CanDeleteEntry) {Image = "table-row-delete"};
        ToggleFilterVisibleAction = new UIToggleAction("ToggleFilterVisible", ToggleFilterVisible, CanToggleFilterVisible) {Image = "filter"};
        ClearUserFilterAction = new UIAction("ClearUserFilter", ClearUserFilter, CanClearUserFilter) {Image = "filter-cancel-2"};
        SelectAllAction = new UIAction("SelectAll", SelectAll);
    }

    protected IViewDomain ViewDomain => ServiceProvider.GetRequiredService<IViewDomain>();

    private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        bool newItems = false;
        bool oldItems = false;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                newItems = true;
                break;
            case NotifyCollectionChangedAction.Remove:
                oldItems = true;
                break;
            case NotifyCollectionChangedAction.Replace:
                oldItems = true;
                newItems = true;
                break;
            case NotifyCollectionChangedAction.Reset:
                OnResetColumns();
                break;
        }

        if (oldItems && e.OldItems != null)
        {
            foreach (var column in e.OldItems.Cast<ColumnPropertyView>())
            {
                OnRemoveColumn(column);
            }
        }
        if (newItems && e.NewItems != null)
        {
            foreach (var column in e.NewItems.Cast<ColumnPropertyView>())
            {
                OnAddColumn(column);
            }
        }
    }

    protected virtual void OnResetColumns()
    {
        AutoCalculateProperties.Clear();
    }

    protected virtual void OnAddColumn(ColumnPropertyView column)
    {
        column.DeclaringType ??= typeof(TEntry);
        Debug.Assert(column.PropertyInfo != null);
        
        if (column.PropertyInfo.GetCustomAttribute<AutoCalculateAttribute>() != null)
        {
            if (!AutoCalculateProperties.Contains(column.Name))
            {
                AutoCalculateProperties.Add(column.Name);
            }
        }
    }

    protected virtual void OnRemoveColumn(ColumnPropertyView column)
    {
        column.DeclaringType ??= typeof(TEntry);
        Debug.Assert(column.PropertyInfo != null);

        if (column.PropertyInfo.GetCustomAttribute<AutoCalculateAttribute>() != null)
        {
            if (AutoCalculateProperties.Contains(column.Name) &&

                // make sure that we don't remove the autocalc. when another column view uses the same PropertyInfo
                Columns.All(c => c.PropertyInfo != column.PropertyInfo))
            {
                AutoCalculateProperties.Remove(column.Name);
            }
        }
    }

    public virtual void Dispose()
    {
        CollectionUnsubscribeEvents();

        Filter.FilterChanged -= Filter_FilterChanged;
        (Filter as FilterSet<TEntry>)?.Dispose();

        Columns.CollectionChanged -= Columns_CollectionChanged;
    }

    public bool InsertAllowed
    {
        get => _insertAllowed && !EntityService.IsReadOnly;
        set => SetProperty(ref _insertAllowed, value);
    }

    public bool ModifyAllowed
    {
        get => _modifyAllowed && !EntityService.IsReadOnly;
        set
        {
            if (!SetProperty(ref _modifyAllowed, value)) return;

            if (_modifyAllowed == false && _insertAllowed)
                InsertAllowed = false;
        }
    }

    public bool DeleteAllowed
    {
        get => _deleteAllowed && !EntityService.IsReadOnly;
        set => SetProperty(ref _deleteAllowed, value);
    }

    /// <summary>
    /// Returns true when deferred save is used.
    ///
    /// This means that a change needs to be saved manually before it is written to the data store.
    /// See also method 'Save'.
    /// </summary>
    public bool IsDeferredSaveActive { get; }

    public IFilterSet<TEntry> Filter { get; }

    public IFilterSetView? FilterView { get; protected set; }

    /// <summary>
    /// Returns true when the filter is visible in the GUI.
    ///
    /// This is meant to be used for an visual filter in e.g. an DataGrid object.
    /// This should not prevent the user from using the filter in general.
    /// </summary>
    public bool IsFilterVisible
    {
        get => _isFilterVisible;
        set => SetProperty(ref _isFilterVisible, value);
    }

    public bool IsLoaded
    {
        get => _isLoaded;
        private set => SetProperty(ref _isLoaded, value);
    }

    /// <summary>
    /// Gets or sets the current sorting for the DataSetView.
    ///
    /// <b>Note:</b> This property will be overriden when a ListView is changed.
    /// To define a default sorting for a page you should by default prefer the <c>SortBy</c> property
    /// behind the <c>ListView</c> property.
    /// </summary>
    public PropertyInfo[]? SortBy
    {
        get => _sortBy;
        set
        {
            if (SetProperty(ref _sortBy, value))
            {
                if (IsLoaded)
                    Refresh();
            }
        }
    }

    /// <summary>
    /// Gets or sets the current sorting of the DataListView.
    ///
    /// This property is only used when <c>SortBy</c> is sed.
    /// </summary>
    public SortDirection SortDirection
    {
        get => _sortDirection;
        set => SetProperty(ref _sortDirection, value);
    }

    public bool CanUserSort
    {
        get => _canUserSort;
        set => SetProperty(ref _canUserSort, value);
    }

    /// <summary>
    /// The properties which will be auto-calculated.
    /// </summary>
    public IList<string> AutoCalculateProperties { get; } = new List<string>();

    /// <summary>
    /// The properties which are visible in the view.
    /// </summary>
    public PropertyViewCollection<TEntry> Columns { get; }

    // TODO: ugly solution, something to undesign; only requrired for WPF; maybe this could be used in connection with a 'has been edited once' state in connection with IEditableObject / EditableEntityBase class
    // NOTE: should never be changed by the user
    public bool LastEditCancelled { get; set; }

    public TEntry? NewEntry
    {
        get => _newEntry;
        private set
        {
            if (_newEntry == value) return;
            if (!LastEditCancelled && IsNewEntry)
            {
                if (HasValidationErrors)
                {
                    // silently remove the entry from the list
                    //Collection.CollectionChanged -= Collection_CollectionChanged;
                    //Collection.Remove(_newEntry);
                    //Collection.CollectionChanged += Collection_CollectionChanged;
                }
                else if (NewEntry != null)
                {
                    // ignore validation errors, throw away the record
                    EntityService.Create(NewEntry);
                }
            }
            IsNewEntry = value != null;
            _newEntry = value;
            OnPropertyChanged();
        }
    }

    public IEntityService<TEntry> GetEntityService()
    {
        return EntityService;
    }
    public TService GetEntityService<TService>()
        where TService : IEntityService<TEntry>
    {
        return (TService)EntityService;
    }

    /// <summary>
    /// Gives back if the current entry is a new entry and it is editable.
    ///
    /// Only in this state it is allowed to change the primary key fields.
    /// </summary>
    public bool IsNewEntry
    {
        get => _isNewEntry;
        private set => SetProperty(ref _isNewEntry, value);
    }

    public TEntry? Current
    {
        get => _current;
        set
        {
            if (_current == value) return;

            if (IsNewEntry && NewEntry != value)
            {
                if (!LastEditCancelled)
                    // this call will save the new entry to the entity service; we don't do this when the creation has been cancelled;
                    NewEntry = null;
            }

            _current = value;
                
            if (_current != null)
                RefreshPropertyLookups(true); // this could maybe be done deferred
            OnPropertyChanged();
        }
    }

    public IList? SelectedEntries
    {
        get => _selectedEntries;
        set => SetProperty(ref _selectedEntries, value);
    }

    public bool HasValidationErrors => _entriesWithValidationErrors.Count > 0;

    #region Static fields cache
    // This cache is implemented to do an import via excel faster.

    private Dictionary<PropertyInfo, object>? _staticFilterCache;
    private bool _staticFilterCacheNeedsRefresh;

    private void LoadStaticFilterCache()
    {
        if (_staticFilterCache == null)
            _staticFilterCache = new Dictionary<PropertyInfo, object>();
        else
            _staticFilterCache.Clear();

        _staticFilterCache.AddRange(
            FilterExpressionParsing.ParseStaticFilters(Filter.FilterExpression)
        );
    }

    private void CheckRefreshStaticFilterCache()
    {
        if (_staticFilterCacheNeedsRefresh || _staticFilterCache == null)
            LoadStaticFilterCache();
    }

    #endregion

    #region Defer refresh logic
        
    private void ReleaseDeferRefresh()
    {
        if (_refreshDeferred)
        {
            Refresh();
            _refreshDeferred = false;
            _refreshPropertyLookupsDeferred = false;
            _refreshPropertyLookupsOnlyOnEntryDeferred = false; 
        }
        else if (_refreshPropertyLookupsDeferred)
        {
            RefreshPropertyLookups(false);
            _refreshPropertyLookupsDeferred = false;
            _refreshPropertyLookupsOnlyOnEntryDeferred = false; 
        }
        else if (_refreshPropertyLookupsOnlyOnEntryDeferred)
        {
            RefreshPropertyLookups(true);
            _refreshPropertyLookupsOnlyOnEntryDeferred = false;
        }
    }

    private class DeferRefreshHolder : IDisposable
    {
        private readonly DataSetView<TEntry> _dataSet;

        public DeferRefreshHolder(DataSetView<TEntry> dataSet)
        {
            _dataSet = dataSet;
        }

        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _dataSet._deferredRefreshes.Remove(this);
            if (_dataSet._deferredRefreshes.Count == 0)
                _dataSet.ReleaseDeferRefresh();
        }
    }

    #endregion

    private void Collection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        bool deleteEntries;
        bool newEntries;

        var oldCurrent = _current;
        var oldCanSaveState = CanSave();

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                deleteEntries = false;
                newEntries = true;
                break;
            case NotifyCollectionChangedAction.Move:
                return;
            case NotifyCollectionChangedAction.Remove:
                deleteEntries = true;
                newEntries = false;
                break;
            case NotifyCollectionChangedAction.Replace:
                // @TODO: when this happen, check if the initEntries should be true or false;
                throw new NotSupportedException(Res.CollectionChangeActionReplaceNotSupported);
            /*
            deleteEntries = true;
            newEntries = true;
            initEntries = false; // We assume that the 'Replace' function is only used when the entry has already been implemented (!)
            break;
            */
            case NotifyCollectionChangedAction.Reset:
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(e.Action));
        }

        if (deleteEntries && e.OldItems != null)
        {
            foreach (TEntry oldEntry in e.OldItems)
            {
                EntryUnsubscribeEvents(oldEntry);

                _current = oldEntry;
                if (!IsNewEntry)
                    RaiseDeletingEntry(oldEntry);

                // remove from internal cache
                _entriesWithValidationErrors.Remove(oldEntry);
            }

            if (IsNewEntry)
            {
                IsNewEntry = false;
                NewEntry = null;
            }
            else
            {
                // required so that the entity service starts tracking the deletion
                if (e.OldItems.Count == 1)
                {
#pragma warning disable CS8604
#pragma warning disable CS8600
                    EntityService.Delete((TEntry)e.OldItems[0]);
#pragma warning restore CS8600
#pragma warning restore CS8604
                }
                else
                {
                    EntityService.DeleteRange(e.OldItems.Cast<TEntry>());
                }
            }
        }

        if (newEntries && e.NewItems != null)
        {
            LastEditCancelled = false;
            foreach (TEntry newEntry in e.NewItems)
            {
                EntrySubscribeEvents(newEntry);

                // disabled because IRowVersion was removed, but I don't know if the code will still work (!)
                //if ((newEntry as IRowVersion)?.RowVersion != null) // TODO [minor] this is a bit dirty here
                //    continue;

                InitNewEntry(newEntry);
                NewEntry = newEntry; // note: the logic to add new entries to the entity service is behind the 'NewEntry' property
                RaiseAddingNewEntry(newEntry);
                _current = newEntry;
                OnPropertyChanged(nameof(Current));
                RaiseAddedNewEntry(newEntry);
            }
        }

        if (_current != oldCurrent)
        {
            RefreshPropertyLookups(true);
        }

        if (!oldCanSaveState)
            RaiseCanSaveChanged();
    }

    private void RefreshPropertyLookups(bool refreshOnlyDependentOnEntry)
    {
        if (_deferredRefreshes.Count > 0)
        {
            if (refreshOnlyDependentOnEntry)
                _refreshPropertyLookupsOnlyOnEntryDeferred = true;
            else
            {
                _refreshPropertyLookupsDeferred = true;
            }

            return;
        }

        Columns.RefreshPropertyLookups(refreshOnlyDependentOnEntry);
    }

    #region Public methods

    public virtual IQueryable<TEntry> AsQueryable()
    {
        var filterExpression = Filter.FilterExpression;

        return filterExpression == null
            ? EntityService.AsQueryable()
            : EntityService.AsQueryable().Where(filterExpression);
    }

    public virtual IQueryable<TEntry> AsQueryableForUpdate()
    {
        var filterExpression = Filter.FilterExpression;

        return filterExpression == null
            ? EntityService.AsQueryableForUpdate()
            : EntityService.AsQueryableForUpdate().Where(filterExpression);
    }

    /// <summary>
    /// A internal function called before the loaded entries
    /// are added to the 'Collection' property, given the option to
    /// manipulate the entries to be added to the list.
    /// </summary>
    /// <param name="newEntries">
    /// The new entries which shall be added to the 'Collection' property.
    /// </param>
    protected virtual void OnLoad(List<TEntry> newEntries)
    {
    }

    public virtual void Load()
    {
        var queryable = (!EntityService.IsReadOnly && (InsertAllowed || ModifyAllowed || DeleteAllowed))
            ? AsQueryableForUpdate()
            : AsQueryable();

        if (typeof(ISortableEntity).IsAssignableFrom(typeof(TEntry)))
        {
            var propertyInfo = typeof(ISortableEntity).GetProperty(nameof(ISortableEntity.SortOrder));
            Debug.Assert(propertyInfo != null);

            // create lambda expression for sorting:
            //   Param_0 => Convert(Param_0, ISortableEntity).SortOrder
            //
            var parameter = Expression.Parameter(typeof(TEntry));
            var orderByExpression = Expression.Lambda(
                Expression.Property(
                    Expression.Convert(
                        parameter,
                        typeof(ISortableEntity)
                    ),
                    propertyInfo
                )
                , parameter
            );

            queryable = queryable.OrderBy((Expression<Func<TEntry, int>>)orderByExpression);
        }
        else if (SortBy != null)
        {
            queryable = SortDirection == SortDirection.Ascending
                ? queryable.OrderBy(SortBy)
                : queryable.OrderByDescending(SortBy);
        }

        if (AutoCalculateProperties.Count > 0)
        {
            var nestedDataFilter = Filter.GetNestedDataFilter(ViewDomain);

            queryable = queryable.AutoCalculate(
                properties: AutoCalculateProperties,
                noTracking: false,
                nestedDataFilter: nestedDataFilter
            );
        }

        var newList = queryable.ToList();

        // this is necessary of today when AutoCalculate(..) is called with noTracking: true, but we want to have tracking.
        // the downside of this code is that it requires IEquatable<TEntry> to be implemented for the entry otherwise it might
        // run into an exception at 'dbContext.Set<TEntry>().Attach(entry);' when this method is called twice for an entry with
        // the same primary key.
        /*if (Repository is DbRepositoryBase<TEntry> dbRepository
            && !dbRepository.IsReadonly // in IsReadonly mode no tracking is taking place, so we can skip it in this case.
            && AutoCalculateProperties.Count > 0)
        {
            // add the object manually to the change tracker because it is removed by the use of queryable.AutoCalculate(..)

            if (!typeof(TEntry).GetInterfaces().Contains(typeof(IEquatable<>).MakeGenericType(typeof(TEntry))))
            {
                throw new NotSupportedException();
            }

            var dbContext = (DataDomainScope as DbDataDomainScope).DbContext;
            var changeTrackingList = dbContext.ChangeTracker.Entries<TEntry>().Select(ee => ee.Entity);

            if (Repository.PrimaryKeyProperties.Count > 0)
            {
                foreach (var entry in newList.Except(changeTrackingList))
                {
                    dbContext.Set<TEntry>().Attach(entry);
                }
            }
            else
            {
                dbContext.Set<TEntry>().AttachRange(newList);
            }
        }*/

        lock (Collection)
        {
            CollectionUnsubscribeEvents();
                
            Collection.Clear();
            _entriesWithValidationErrors.Clear();
            OnLoad(newList);
            Collection.AddRange(newList);

            CollectionSubscribeEvents();
        }
            
        if (!IsLoaded)
        {
            IsLoaded = true;
        }
    }

    public IDisposable DeferRefresh()
    {
        var deferObject = new DeferRefreshHolder(this);
        _deferredRefreshes.Add(deferObject);
        return deferObject;
    }

    public virtual void Refresh()
    {
        if (_deferredRefreshes.Count > 0)
        {
            _refreshDeferred = true;
            return;
        }

        var oldCurrent = Current;

        Load();

        if (oldCurrent != null &&
            // We can only assure that the Equals<> options works as expected when
            // the IEquatable<T> interface is implemented.
            typeof(IEquatable<TEntry>).IsAssignableFrom(typeof(TEntry)))
        {
            Current = Collection.FirstOrDefault(e => e.Equals(oldCurrent));
        }

        RefreshPropertyLookups(false);
    }

    private void CheckIsLoaded([CallerMemberName] string? methodName = default)
    {
        if (!IsLoaded)
            throw new NotSupportedException($"You have to call first {nameof(Load)}() before calling method {methodName}");
    }

    private void CheckInsertAllowed([CallerMemberName] string? methodName = default)
    {
        if (!InsertAllowed)
            throw new NotSupportedException($"Insert is currently not allowed to this DataSetView. Caller method: {methodName}");
    }

    private void CheckDeleteAllowed([CallerMemberName] string? methodName = default)
    {
        if (!DeleteAllowed)
            throw new NotSupportedException($"Delete is currently not allowed to this DataSetView. Caller method: {methodName}");
    }

    public void Add(TEntry entry)
    {
        CheckInsertAllowed();
        CheckIsLoaded();
        Collection.Add(entry);
    }

    public void AddRange(IEnumerable<TEntry> entries)
    {
        CheckInsertAllowed();
        CheckIsLoaded();
        using var deferRefresh = DeferRefresh();
        Collection.AddRange(entries);
    }

    public void Remove(TEntry entry)
    {
        CheckDeleteAllowed();
        CheckIsLoaded();
        Collection.Remove(entry);
    }

    public void RemoveRange(IEnumerable<TEntry> entries)
    {
        CheckDeleteAllowed();
        CheckIsLoaded();
        using var deferRefresh = DeferRefresh();
        Collection.RemoveRange(entries);
    }

    /// <summary>
    /// Will be executed when a new row is initialized before it is added to the collection.
    /// </summary>
    /// <param name="entry">
    /// The new entry which shall be initialized.
    /// </param>
    public virtual void InitNewEntry(TEntry entry)
    {
        EntityService.Init(entry);

        // init entry fields from filter predicate

        CheckRefreshStaticFilterCache();

#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
        foreach (var cache in _staticFilterCache)
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
        {
            cache.Key.SetMethod?.Invoke(entry, new[] {cache.Value});
        }
    }

    public event EventHandler? CanSaveChanged;

    private void RaiseCanSaveChanged()
    {
        CanSaveChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanSave()
    {
        if (HasValidationErrors)
            return false;

        if (!IsDeferredSaveActive) return false;

        if (IsNewEntry)
            return true;

        if (EntityService is IEntityDeferredCommitService deferredCommitService)
        {
            if (deferredCommitService.CanSave())
                return true;
        }
        else if (DataDomainScope.GetType().Name == "EFCoreDataDomainScope")
        {
            // Hacky code:
            // If the entity service does not implement IEntityDeferredCommitService, it will write directly to the
            // EF Core change tracker. For such cases, we always return 'true' to allow the user
            // to execute `dbContext.SaveChanges()`.
            return true;
        }

        return DataDomainScope.CanSave();
    }

    public virtual void PrepareSave()
    {
        NewEntry = null;
    }

    public void Save()
    {
        PrepareSave();

        // TODO: here we save all changes, not just the changes for this DataSet. This might raise confusion and should be changed.
        DataDomainScope.Save();
    }

    public virtual void RejectChanges()
    {
        if (EntityService is IEntityDeferredCommitService deferredCommitService)
        {
            deferredCommitService.RejectChanges();
            Refresh();
        }
        else
        {
            DataDomainScope.RejectChanges();
        }
    }

    #endregion

    #region Actions

    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Display(Name = "CreateNewEntryCommand_Name", Description = "CreateNewEntryCommand_Description", ResourceType = typeof(Res))]
    public IAction CreateNewEntryAction { get; }

    [Display(Name = "DeleteEntryCommand_Name", Description = "DeleteEntryCommand_Description", ResourceType = typeof(Res))]
    public IDataSetSelectionAction<TEntry> DeleteEntryAction { get; }

    [Display(Name = "FilterVisibleToggleCommand_Name", Description = "FilterVisibleToggleCommand_Description", ResourceType = typeof(Res))]
    public IToggleAction ToggleFilterVisibleAction { get; }

    [Display(Name = "ClearUserFilterCommand_Name", Description = "ClearUserFilterCommand_Description", ResourceType = typeof(Res))]
    public IAction ClearUserFilterAction { get; }

    [Display(Name = "SortAscendingCommand_Name", Description = "SortAscendingCommand_Description", ResourceType = typeof(Res))]
    public IAction? SortAscendingAction { get; }

    [Display(Name = "SortDescendingCommand_Name", Description = "SortDescendingCommand_Description", ResourceType = typeof(Res))]
    public IAction? SortDescendingAction { get; }

    public IAction SelectAllAction { get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global

    protected virtual bool CanCreateNewEntry()
    {
        if (!InsertAllowed)
            return false;

        if (IsNewEntry)
        {
            if (NewEntry is INotifyDataErrorInfo notifyDataErrorInfo)
            {
                if (notifyDataErrorInfo.HasErrors)
                    return false;
            }
        }

        return true;
    }

    protected virtual void CreateNewEntry()
    {
        if (IsNewEntry && NewEntry is INotifyDataErrorInfo notifyDataErrorInfo && notifyDataErrorInfo.HasErrors)
        {
            ViewDomain.ShowErrorMessage(
                Res.Error_CreateNewEntryWhileCurrentHasValidationError_Message,
                Res.Error_CreateNewEntryWhileCurrentHasValidationError_Title
                // NOTE: owning page is not given here because we don't know the owning page in the DataSetView
            );
            return;
        }

        var newEntry = new TEntry();

        Collection.Add(newEntry);
    }

    protected virtual bool CanDeleteEntry(IList<TEntry?>? selectedEntries)
    {
        return DeleteAllowed && selectedEntries?.Count > 0;
    }

    protected virtual void DeleteEntry(IList<TEntry?>? selectedEntries)
    {
        if (selectedEntries == null) return;

        using var deferRefresh = DeferRefresh();

        var entriesToDelete = (from e in selectedEntries.AsQueryable().Cast<object>()
            where e is TEntry // Ignore NewItemPlaceholderType
            select (TEntry) e).ToList();

        var nextIndex = (from e in entriesToDelete
            select Collection.IndexOf(e)).Max();

        Collection.RemoveRange(entriesToDelete);

        if (Collection.Count == 0 || nextIndex == -1)
        {
            Current = null;
        }
        else
        {
            if (nextIndex >= Collection.Count)
            {
                nextIndex = Collection.Count - 1;
            }

            Current = Collection[nextIndex];
        }
    }

    protected virtual bool CanToggleFilterVisible()
    {
        return false;
    }
    protected virtual void ToggleFilterVisible()
    {
        throw new NotImplementedException();
    }

    protected virtual bool CanClearUserFilter()
    {
        return Filter.UserLayer.Properties.Count > 0;
    }

    protected virtual void ClearUserFilter()
    {
        Filter.UserLayer.Clear();
        Refresh();
    }

    protected virtual bool CanSortAscending()
    {
        return SortBy != null && SortDirection == SortDirection.Descending;
    }

    protected virtual void SortAscending()
    {
        SortDirection = SortDirection.Ascending;
        Refresh();
    }

    protected virtual bool CanSortDescending()
    {
        return SortBy != null && SortDirection == SortDirection.Ascending;
    }

    protected virtual void SortDescending()
    {
        SortDirection = SortDirection.Descending;
        Refresh();
    }

    protected virtual void SelectAll()
    {
        SelectedEntries = Collection.ToList();
    }

    #endregion

    #region Internal event methods
        
    protected virtual void OnFilterChanged()
    {
        _staticFilterCacheNeedsRefresh = true;

        if (!IsLoaded)
            return;

        PrepareSave();
        Save();

        Refresh();
    }

    protected virtual void OnEntryErrorsChanged(TEntry entry, string? propertyName)
    {
        if (((INotifyDataErrorInfo) entry).HasErrors)
        {
            if (!_entriesWithValidationErrors.Contains(entry))
            {
                _entriesWithValidationErrors.Add(entry);

                if (_entriesWithValidationErrors.Count == 1)
                    OnPropertyChanged(nameof(HasValidationErrors));
            }
        }
        else
        {
            if (_entriesWithValidationErrors.Contains(entry))
            {
                _entriesWithValidationErrors.Remove(entry);

                if (_entriesWithValidationErrors.Count == 0)
                    OnPropertyChanged(nameof(HasValidationErrors));
            }
        }
    }

    protected virtual void OnEntryPropertyChanging(TEntry entry, string? propertyName)
    {
        if (!typeof(TEntry).IsSubclassOf(typeof(EditableEntityBase)))
            EntityService.OnPropertyChanging(entry, propertyName);
    }

    protected virtual void OnEntryPropertyChanged(TEntry entry, string? propertyName)
    {
        if (!typeof(TEntry).IsSubclassOf(typeof(EditableEntityBase)))
            EntityService.OnPropertyChanged(entry, propertyName);

        if (entry == Current)
        {
            RefreshPropertyLookups(true);
        }
    }

    protected virtual void OnAddingNewEntry(TEntry entry)
    {
    }

    protected virtual void OnAddedNewEntry(TEntry entry)
    {
    }

    protected virtual void OnDeletingEntry(TEntry entry)
    {
    }

    #endregion

    #region Event handling

    public event EventHandler<AddingNewEntryEventArgs>? AddingNewEntry;

    public event EventHandler<NewEntryAddedEventArgs>? NewEntryAdded;

    public event EventHandler<DeletingEntryEventArgs>? DeletingEntry;

    public event EventHandler<EntryPropertyChangingEventArgs>? EntryPropertyChanging;

    public event EventHandler<EntryPropertyChangedEventArgs>? EntryPropertyChanged;

    public event EventHandler<EntryDataErrorsChangedEventArgs>? EntryErrorsChanged;

    public event EventHandler<DataSetEntityColoringEventArgs>? EntryColoring;

    private void Entry_PropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        if (sender == null) return;
        OnEntryPropertyChanging((TEntry)sender, e.PropertyName);
        EntryPropertyChanging?.Invoke(this, new EntryPropertyChangingEventArgs(sender, e.PropertyName));
    }

    private void Entry_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender == null) return;
        OnEntryPropertyChanged((TEntry)sender, e.PropertyName);
        EntryPropertyChanged?.Invoke(this, new EntryPropertyChangedEventArgs(sender, e.PropertyName));
    }

    private void Entry_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        if (sender == null) return;
        OnEntryErrorsChanged((TEntry)sender, e.PropertyName);
        EntryErrorsChanged?.Invoke(this, new EntryDataErrorsChangedEventArgs((TEntry)sender, e.PropertyName));
    }

    private void Filter_FilterChanged(object? sender, EventArgs e)
    {
        OnFilterChanged();
    }

    private void RaiseAddingNewEntry(TEntry entry)
    {
        OnAddingNewEntry(entry);
        AddingNewEntry?.Invoke(this, new AddingNewEntryEventArgs(entry));
    }

    private void RaiseAddedNewEntry(TEntry entry)
    {
        OnAddingNewEntry(entry);
        NewEntryAdded?.Invoke(this, new NewEntryAddedEventArgs(entry));
    }

    private void RaiseDeletingEntry(TEntry entry)
    {
        OnDeletingEntry(entry);
        DeletingEntry?.Invoke(this, new DeletingEntryEventArgs(entry));
    }

    protected void RaiseEntryColoring(DataSetEntityColoringEventArgs eventArgs)
    {
        EntryColoring?.Invoke(this, eventArgs);
    }

    private void CollectionSubscribeEvents()
    {
        Collection.CollectionChanged += Collection_CollectionChanged;

        if (typeof(ISortableEntity).IsAssignableFrom(typeof(TEntry)))
        {
            SortableDataSetView.AddBusinessLogic(this);
        }
            
        bool basedOnEditableEntityViewModelBase =
            typeof(TEntry).IsSubclassOf(typeof(EditableEntityBase));
        bool implementsINotifyPropertyChanging =
            typeof(INotifyPropertyChanging).IsAssignableFrom(typeof(TEntry));
        bool implementsINotifyPropertyChanged =
            typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(TEntry));
        bool implementsINotifyDataErrorInfo =
            typeof(INotifyDataErrorInfo).IsAssignableFrom(typeof(TEntry));

        if (implementsINotifyPropertyChanging ||
            implementsINotifyPropertyChanged)
        {
            foreach (var entry in Collection)
            {
                if (basedOnEditableEntityViewModelBase)
#pragma warning disable CS8604
                    EditableEntityBase.SetBusinessLayerService(entry as EditableEntityBase, EntityService);
#pragma warning restore CS8604
                if (implementsINotifyPropertyChanging)
                    ((INotifyPropertyChanging)entry).PropertyChanging += Entry_PropertyChanging;
                if (implementsINotifyPropertyChanged)
                    ((INotifyPropertyChanged)entry).PropertyChanged += Entry_PropertyChanged;
                if (implementsINotifyDataErrorInfo)
                    ((INotifyDataErrorInfo)entry).ErrorsChanged += Entry_ErrorsChanged;
            }
        }
    }

    private void CollectionUnsubscribeEvents()
    {
        Collection.CollectionChanged -= Collection_CollectionChanged;
            
        if (typeof(ISortableEntity).IsAssignableFrom(typeof(TEntry)))
        {
            SortableDataSetView.RemoveBusinessLogic(this);
        }
            
        bool basedOnEditableEntityViewModelBase =
            typeof(TEntry).IsSubclassOf(typeof(EditableEntityBase));
        bool implementsINotifyPropertyChanging =
            typeof(INotifyPropertyChanging).IsAssignableFrom(typeof(TEntry));
        bool implementsINotifyPropertyChanged =
            typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(TEntry));
        bool implementsINotifyDataErrorInfo =
            typeof(INotifyDataErrorInfo).IsAssignableFrom(typeof(TEntry));

        if (implementsINotifyPropertyChanging ||
            implementsINotifyPropertyChanged)
        {
            foreach (var entry in Collection)
            {
                if (basedOnEditableEntityViewModelBase)
#pragma warning disable CS8604
                    EditableEntityBase.SetBusinessLayerService(entry as EditableEntityBase, null);
#pragma warning restore CS8604
                if (implementsINotifyPropertyChanging)
                    ((INotifyPropertyChanging) entry).PropertyChanging -= Entry_PropertyChanging;
                if (implementsINotifyPropertyChanged)
                    ((INotifyPropertyChanged) entry).PropertyChanged -= Entry_PropertyChanged;
                if (implementsINotifyDataErrorInfo)
                    ((INotifyDataErrorInfo) entry).ErrorsChanged -= Entry_ErrorsChanged;
            }
        }
    }

    private void EntrySubscribeEvents(TEntry entry)
    {
        if (entry is EditableEntityBase entryViewModel)
            EditableEntityBase.SetBusinessLayerService(entryViewModel, EntityService);
        if (entry is INotifyPropertyChanging newEntryNotifyPropertyChanging)
            newEntryNotifyPropertyChanging.PropertyChanging += Entry_PropertyChanging;
        if (entry is INotifyPropertyChanged newEntryNotifyPropertyChanged)
            newEntryNotifyPropertyChanged.PropertyChanged += Entry_PropertyChanged;
        if (entry is INotifyDataErrorInfo newEntryNotifyDataErrorInfo)
            newEntryNotifyDataErrorInfo.ErrorsChanged += Entry_ErrorsChanged;
    }

    private void EntryUnsubscribeEvents(TEntry entry)
    {
        if (entry is EditableEntityBase entryViewModel)
            EditableEntityBase.SetBusinessLayerService(entryViewModel, null);
        if (entry is INotifyPropertyChanging oldEntryNotifyPropertyChanging)
            oldEntryNotifyPropertyChanging.PropertyChanging -= Entry_PropertyChanging;
        if (entry is INotifyPropertyChanged oldEntryNotifyPropertyChanged)
            oldEntryNotifyPropertyChanged.PropertyChanged -= Entry_PropertyChanged;
        if (entry is INotifyDataErrorInfo oldEntryNotifyDataErrorInfo)
            oldEntryNotifyDataErrorInfo.ErrorsChanged -= Entry_ErrorsChanged;
    }

    #endregion

    #region IDataSetReadonlyView

    IFilterSet IDataSetReadonlyView.Filter => Filter;

    object? IDataSetReadonlyView.Current
    {
        get => Current;
        set => Current = (TEntry?)value;
    }

    IQueryable IDataSetReadonlyView.AsQueryable()
    {
        return AsQueryable();
    }

    IPropertyViewCollection IDataSetReadonlyView.Columns => Columns;
        
    #endregion

    #region IDataSetReadonlyView<TEntry>

    IList<TEntry>? IDataSetReadonlyView<TEntry>.SelectedEntries
    {
        get => SelectedEntries?.Cast<TEntry>().ToList();
        set => SelectedEntries = value as IList;
    }

    ICollection<TEntry> IDataSetReadonlyView<TEntry>.Collection => Collection;

    IPropertyViewCollection<TEntry> IDataSetReadonlyView<TEntry>.Columns => this.Columns;

    #endregion

    #region IDataSetView

    void IDataSetView.Add(object entry)
    {
        if (!(entry is TEntry entryT))
        {
            throw new ArgumentException(
                string.Format(Res.ParameterMustBeOfType, nameof(entry), typeof(TEntry).FullName), nameof(entry));
        }

        Add(entryT);
    }

    void IDataSetView.AddRange(IEnumerable<object> entries)
    {
        if (!(entries is IEnumerable<TEntry> entriesT))
        {
            throw new ArgumentException(
                string.Format(Res.ParameterMustBeOfType, nameof(entries), typeof(IEnumerable<TEntry>).FullName),
                nameof(entries));
        }

        AddRange(entriesT);
    }

    void IDataSetView.Remove(object entry)
    {
        if (!(entry is TEntry entryT))
        {
            throw new ArgumentException(
                string.Format(Res.ParameterMustBeOfType, nameof(entry), typeof(TEntry).FullName), nameof(entry));
        }

        Remove(entryT);
    }

    void IDataSetView.RemoveRange(IEnumerable<object> entries)
    {
        if (!(entries is IEnumerable<TEntry> entriesT))
        {
            throw new ArgumentException(
                string.Format(Res.ParameterMustBeOfType, nameof(entries), typeof(IEnumerable<TEntry>).FullName),
                nameof(entries));
        }

        RemoveRange(entriesT);
    }

    void IDataSetView.InitNewEntry(object entry)
    {
        if (!(entry is TEntry entryT))
        {
            throw new ArgumentException(
                string.Format(Res.ParameterMustBeOfType, nameof(entry), typeof(TEntry).FullName), nameof(entry));
        }

        InitNewEntry(entryT);
    }

    IDataSetSelectionAction IDataSetView.DeleteEntryAction => (IDataSetSelectionAction)DeleteEntryAction;

    IQueryable IDataSetView.AsQueryableForUpdate() => AsQueryableForUpdate();

    #endregion
}