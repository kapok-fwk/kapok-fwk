namespace Kapok.View;

public interface ICardPage : IDataPage
{
}

public interface ICardPage<TEntry> : IDataPage<TEntry>
    where TEntry : class, new()
{
    IList<PropertyView> PropertyViewDefinitions { get; }
}