namespace Kapok.View;

public interface ILinkedDetailPage<TBaseEntry, TLinkedEntry> : IDetailPage<TLinkedEntry>
    where TBaseEntry : class, new()
    where TLinkedEntry : class, new()
{
    IDataSetView<TBaseEntry> SourceDataSet { get; }
}