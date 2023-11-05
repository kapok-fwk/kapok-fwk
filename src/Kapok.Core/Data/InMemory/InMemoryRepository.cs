namespace Kapok.Data.InMemory;

/// <summary>
/// A in-memory repository holding the entities in memory.
///
/// Note: The in-memory repository does not have any primary key or unique key check.
/// </summary>
/// <typeparam name="T"></typeparam>
public class InMemoryRepository<T> : IRepository<T>
    where T : class
{
    private readonly List<T> _list;

    public ICollection<string>? IncludeNestedData => null;

    /// <summary>
    /// A memory repository holding the data until the scope is disposed.
    /// </summary>
    public InMemoryRepository()
    {
        _list = new List<T>();
    }

    /// <summary>
    /// A memory repository with a reference to the data.
    /// </summary>
    /// <param name="inMemoryList"></param>
    public InMemoryRepository(List<T> inMemoryList)
    {
        ArgumentNullException.ThrowIfNull(inMemoryList);
        _list = inMemoryList;
    }

    /// <summary>
    /// A memory repository holding the data based on the lifetime of the data domain.
    /// </summary>
    /// <param name="dataDomain"></param>
    public InMemoryRepository(IDataDomain dataDomain)
    {
        _list = ((InMemoryDataDomain)dataDomain).GetInMemoryData<T>();
    }

    public virtual IQueryable<T> AsQueryable()
    {
        return _list.AsQueryable();
    }

    public virtual void Create(T entity)
    {
        _list.Add(entity);
    }

    public virtual void Update(T entity, T? originalEntity)
    {
        // nothing to do; all is already stored in memory
    }

    public virtual void Delete(T entity)
    {
        _list.Remove(entity);
    }

    public virtual void CreateRange(IEnumerable<T> entities)
    {
        _list.AddRange(entities);
    }

    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        _list.RemoveRange(entities);
    }

    public virtual Task CreateAsync(T entity)
    {
        Create(entity);
        return Task.CompletedTask;
    }

    public virtual Task UpdateAsync(T entity, T? originalEntity)
    {
        Update(entity, originalEntity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity)
    {
        Delete(entity);
        return Task.CompletedTask;
    }

    public virtual Task CreateRangeAsync(IEnumerable<T> entities)
    {
        CreateRange(entities);
        return Task.CompletedTask;
    }

    public virtual Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        DeleteRange(entities);
        return Task.CompletedTask;
    }
}