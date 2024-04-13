using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Kapok.BusinessLayer;

namespace Kapok.View;

public interface IDataSetReadonlyView : INotifyPropertyChanged
{
    // data view and loading
    bool IsLoaded { get; }
    IList<string> AutoCalculateProperties { get; }
    IPropertyViewCollection Columns { get; }
    void Load();
    void Refresh();

    // interaction with DataSet
    object? Current { get; set; }
    IList? SelectedEntries { get; set; }
    IQueryable AsQueryable();
    IToggleAction ToggleFilterVisibleAction { get; }
    IAction ClearUserFilterAction { get; }


    // filter properties
    IFilterSet Filter { get; }

    IFilterSetView? FilterView { get; }

    bool IsFilterVisible { get; set; }

    // index and sorting
    bool CanUserSort { get; set; }
    PropertyInfo[]? SortBy { get; set; }
    SortDirection SortDirection { get; set; }
    IAction? SortAscendingAction { get; }
    IAction? SortDescendingAction { get; }
}

public interface IDataSetReadonlyView<TEntry> : IDataSetReadonlyView
    where TEntry : class, new()
{
    IEntityService<TEntry> GetEntityService();
    TService GetEntityService<TService>()
        where TService : IEntityService<TEntry>;

    new IPropertyViewCollection<TEntry> Columns { get; }
    new TEntry? Current { get; set; }
    new IList<TEntry>? SelectedEntries { get; set; }
    new IFilterSet<TEntry> Filter { get; }

    /// <summary>
    /// Returns a IDisposable object. Refreshing of the DataSet is hold until the
    /// object is disposed.
    /// </summary>
    /// <returns></returns>
    IDisposable DeferRefresh();

    new IQueryable<TEntry> AsQueryable();

    ICollection<TEntry> Collection { get; }
}