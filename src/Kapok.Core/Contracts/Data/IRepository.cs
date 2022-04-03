namespace Kapok.Core;

public interface IRepository<T> : IReadOnlyRepository<T>
    where T : class
{
    void Create(T entity);

    /// <summary>
    /// Updates an entity.
    ///
    /// When <para>originalEntity</para> is given, the repository can decide if the whole entity
    /// shall be updated or just the changed properties between <para>entity</para> and
    /// <para>originalEntity</para>.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="originalEntity"></param>
    void Update(T entity, T? originalEntity);
    void Delete(T entity);
    void CreateRange(IEnumerable<T> entities);
    void DeleteRange(IEnumerable<T> entities);

    Task CreateAsync(T entity);
    Task UpdateAsync(T entity, T? originalEntity);
    Task DeleteAsync(T entity);
    Task CreateRangeAsync(IEnumerable<T> entities);
    Task DeleteRangeAsync(IEnumerable<T> entities);
}