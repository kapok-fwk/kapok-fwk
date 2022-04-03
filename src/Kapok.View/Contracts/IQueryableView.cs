namespace Kapok.View;

public interface IQueryableView<T>
    where T : class
{
    IQueryable<T> QueryableSource { get; }

    void Refresh();
}