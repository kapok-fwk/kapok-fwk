namespace Kapok.View;

public interface IDetailPage : IDataPage
{
    bool CanClose { get; set; }

    bool IsClosed { get; set; }
}

// ReSharper disable once UnusedTypeParameter
public interface IDetailPage<TEntry> : IDetailPage
    where TEntry : class, new()
{
}