﻿using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Kapok.Data;
using Kapok.Entity;
using Kapok.Entity.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Kapok.View;

// TODO: planned to implement here INotifyPropertyChanged, INotifyCollectionChanged
public class PropertyViewCollection<TEntity> : IPropertyViewCollection<TEntity>
    where TEntity : class, new()
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityModel _entityModel;
    private readonly IDataSetView<TEntity>? _dataSet;
    private readonly Func<TEntity?>? _currentSelector;
        
    private readonly ObservableCollection<PropertyView> _observableCollection = new();

    private readonly Dictionary<string, IPropertyLookupView> _lookupViews = new();
    private readonly Dictionary<string, IDataSetSelectionAction<TEntity>> _drillDown = new();

    public PropertyViewCollection(IServiceProvider serviceProvider,
        IEntityModel entityModel, Func<TEntity?>? currentSelector = null)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(entityModel);
        _serviceProvider = serviceProvider;
        _entityModel = entityModel;
        _currentSelector = currentSelector;
    }

    public PropertyViewCollection(IServiceProvider serviceProvider,
        IEntityModel entityModel, IDataSetView<TEntity>? baseDataSet = null)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(entityModel);
        _serviceProvider = serviceProvider;
        _entityModel = entityModel;
        _currentSelector = () => baseDataSet?.Current;
        _dataSet = baseDataSet;
    }

    public void RefreshPropertyLookups(bool refreshOnlyDependentOnEntry)
    {
        IEnumerable<IPropertyLookupView> propertyLookupViewEnumerable =
            refreshOnlyDependentOnEntry
                ? _lookupViews.Values.Where(p => p.LookupDefinition.EntriesFuncDependentOnEntry)
                : _lookupViews.Values;

        foreach (var propertyLookupView in propertyLookupViewEnumerable)
        {
            Debug.WriteLine(
                $"Reload LookupView for {_lookupViews.AsQueryable().First(p => p.Value == propertyLookupView).Key}");

            propertyLookupView.Refresh();
        }
    }

    /// <summary>
    /// Property lookup views.
    /// </summary>
    public IReadOnlyDictionary<string, IPropertyLookupView> LookupViews => _lookupViews;

    /// <summary>
    /// Property drill-down actions
    /// </summary>
    public IReadOnlyDictionary<string, IDataSetSelectionAction<TEntity>> DrillDown => _drillDown;

    protected virtual void OnAdd(PropertyView item)
    {
        var viewDomain = _serviceProvider.GetRequiredService<IViewDomain>();

        // NOTE: move this maybe into the PropertyView constructor?
        item.LookupDefinition ??= _entityModel.Properties
            .FirstOrDefault(p => p.PropertyName == item.Name)?.LookupDefinition;

        if (item.LookupDefinition != null)
        {
            // NOTE: here we make sure that when a property is used twice we just load it once... TODO maybe something to improve/change? e.g. with a dedicated 'Name' property in the PropertyView class
            if (!_lookupViews.ContainsKey(item.Name))
            {
                var lookupView = viewDomain.CreatePropertyLookupView(item.LookupDefinition, _serviceProvider.GetRequiredService<IDataDomain>(), _currentSelector);
                if (lookupView != null)
                    _lookupViews.Add(item.Name, lookupView);
            }
        }

        // NOTE: move this maybe into the PropertyView constructor?
        item.DrillDownDefinition ??= _entityModel.Properties
            .FirstOrDefault(p => p.PropertyName == item.Name)?.DrillDownDefinition;

        if (item.DrillDownDefinition != null &&
                    
            // TODO: this is (maybe only) temporary necessary because when a card is open a second time with the TableData instance of the list, here this would crash because the propertyView object is already initialized
            !_drillDown.ContainsKey(item.Name)

           )
        {
            var drillDownActionName = $"DrillDownOn{item.Name}";
            IDataSetSelectionAction<TEntity>? drillDownAction;

            /*if (newItem.DrillDownDefinition.DrillDownAction != null)
            {
                throw new NotImplementedException();
                //drillDownAction = new UIAction(drillDownActionName, newItem.DrillDownDefinition.DrillDownAction);
            }
            else*/ if (_dataSet != null)
            {
                var pageType = item.DrillDownDefinition.PageType;
                if (typeof(EntityBase).IsAssignableFrom(pageType))
                {
                    // it is an entity type --> let's get the default IPage object for the type

                    pageType = viewDomain.GetEntityDefaultPageType(pageType);

                    if (pageType == null)
                        throw new NotSupportedException(
                            $"The entity type {item.DrillDownDefinition.PageType.FullName} has no default page type. Error during drill down configuration of the property {item.Name}.");
                }

                if (!typeof(IDataPage).IsAssignableFrom(pageType))
                {
                    throw new NotSupportedException(
                        $"The page type {pageType.FullName} for the drill down of property {item.Name} must inherit the {typeof(IDataPage)} interface.");
                }

                drillDownAction = new UIOpenReferencedPageAction<TEntity>(
                    drillDownActionName,
                    pageType,
                    _serviceProvider,
                    _dataSet,
                    item.DrillDownDefinition.Filter);
            }
            else
            {
                drillDownAction = null;
            }

            if (drillDownAction != null)
                _drillDown.Add(item.Name, drillDownAction);
        }
    }

    protected virtual void OnRemove(PropertyView item)
    {
        // NOTE: when we would use a property twice, this would cause the property lookup to fail when the first one is deleted!
        if (_lookupViews.ContainsKey(item.Name))
            _lookupViews.Remove(item.Name);
    }
        
    #region IList<PropertyView>

    public int IndexOf(PropertyView item)
    {
        return _observableCollection.IndexOf(item);
    }

    public void Insert(int index, PropertyView item)
    {
        OnAdd(item);
        _observableCollection.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        if (index >= 0 && _observableCollection.Count < index)
            OnRemove(_observableCollection[index]);

        _observableCollection.RemoveAt(index);
    }

    public PropertyView this[int index]
    {
        get => _observableCollection[index];
        set => _observableCollection[index] = value;
    }

    #endregion

    #region ICollection<PropertyView>

    public int Count => _observableCollection.Count;

    bool ICollection<PropertyView>.IsReadOnly => false;
        
    public void Add(PropertyView item)
    {
        OnAdd(item);
        _observableCollection.Add(item);
    }

    public void Clear()
    {
        _observableCollection.Clear();
    }

    public bool Contains(PropertyView item)
    {
        return _observableCollection.Contains(item);
    }

    public void CopyTo(PropertyView[] array, int arrayIndex)
    {
        _observableCollection.CopyTo(array, arrayIndex);
    }

    public bool Remove(PropertyView item)
    {
        if (Contains(item))
            OnRemove(item);
        return _observableCollection.Remove(item);
    }

    #endregion
        
    #region IList

    int IList.Add(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        this.Add((PropertyView)value);
        return this.IndexOf((PropertyView)value);
    }
        
    bool IList.Contains(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value is PropertyView propertyView)
            return this.Contains(propertyView);
            
        return false;
    }

    int IList.IndexOf(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value is PropertyView propertyView)
            return this.IndexOf(propertyView);

        return -1;
    }

    void IList.Insert(int index, object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        this.Insert(index, (PropertyView)value);
    }
        
    void IList.Remove(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        this.Remove((PropertyView) value);
    }

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => false;

    object IList.this[int index]
    {
        get => this[index];
#pragma warning disable CS8769
        set
#pragma warning restore CS8769
        {
            ArgumentNullException.ThrowIfNull(value);
            this[index] = (PropertyView)value;
        }
    }

    #endregion
        
    #region ICollection

    void ICollection.CopyTo(Array array, int index)
    {
        ((ICollection)_observableCollection).CopyTo(array, index);
    }

    bool ICollection.IsSynchronized => true;

    object ICollection.SyncRoot => _observableCollection;

    #endregion

    public IEnumerator<PropertyView> GetEnumerator()
    {
        return _observableCollection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #region INotifyCollectionChanged

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => _observableCollection.CollectionChanged += value;
        remove => _observableCollection.CollectionChanged -= value;
    }

    #endregion
}