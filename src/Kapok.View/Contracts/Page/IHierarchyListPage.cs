namespace Kapok.View;

public interface IHierarchyListPage : IListPage
{
    // actions
    IAction MoveInEntryAction { get; }
    IAction MoveOutEntryAction { get; }
}

public interface IHierarchyListPage<TEntry> : IHierarchyListPage, IListPage<TEntry>
    where TEntry : class, new()
{
}