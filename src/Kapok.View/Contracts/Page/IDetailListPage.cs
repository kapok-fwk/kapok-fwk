namespace Kapok.View;

public interface IDetailListPage : IDetailPage
{
}

public interface IDetailListPage<TEntry, TSubEntry> : IDetailPage<TEntry>
    where TEntry : class, new()
    where TSubEntry : class, new()
{
    IDataSetSelectionAction<TSubEntry>? OpenCardPageAction { get; }
}