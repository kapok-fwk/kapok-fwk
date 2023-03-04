namespace Kapok.Data;

public interface IReadOnlyRepository<out T>
    where T : class
{
    /// <summary>
    /// Here you can specify which nested data of the `T` object shall be explicitly loaded.
    /// </summary>
    ICollection<string>? IncludeNestedData { get; }

    IQueryable<T> AsQueryable();
}