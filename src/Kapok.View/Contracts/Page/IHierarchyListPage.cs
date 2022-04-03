namespace Kapok.View;

public interface IHierarchyListPage : IListPage
{
}

public interface IHierarchyListPage<TEntry> : IHierarchyListPage, IListPage<TEntry>
    where TEntry : class, new()
{
}