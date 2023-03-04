namespace Kapok.View;

public interface IListPage : IDataPage
{
    IEnumerable<IDataSetListView> ListViews { get; }

    IDataSetListView? CurrentListView { get; set; }
}

// ReSharper disable once PossibleInterfaceMemberAmbiguity
public interface IListPage<TEntry> : IListPage, IDataPage<TEntry>
    where TEntry : class, new()
{
    IDataSetSelectionAction<TEntry>? OpenCardPageAction { get; }
}