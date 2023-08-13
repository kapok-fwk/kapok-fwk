namespace Kapok.View;

public interface IListPage : IDataPage
{
    IEnumerable<IDataSetListView> ListViews { get; }

    IDataSetListView? CurrentListView { get; set; }
    
    // actions
    IAction EditEntryAction { get; }
    IAction? SortUpEntryAction { get; }
    IAction? SortDownEntryAction { get; }
    IAction ExportAsExcelSheetAction { get; }
    IToggleAction ToggleFilterVisibleAction { get; }
    IAction ClearUserFilterAction { get; }
}

// ReSharper disable once PossibleInterfaceMemberAmbiguity
public interface IListPage<TEntry> : IListPage, IDataPage<TEntry>
    where TEntry : class, new()
{
    IDataSetSelectionAction<TEntry>? OpenCardPageAction { get; }
    
    // actions
    new IDataSetSelectionAction<TEntry> EditEntryAction { get; }
}