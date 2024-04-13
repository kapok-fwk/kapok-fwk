using Kapok.Data;
using Kapok.Entity;
using Kapok.Entity.Model;

namespace Kapok.BusinessLayer;

public class EntityLinqQueryService<T> : IEntityService<T>
    where T : class, new()
{
    public IQueryable<T>? Query { get; set; }

    public EntityLinqQueryService(IQueryable<T>? query = null)
    {
        Query = query;
    }

    public IEntityModel Model => EntityBase.GetEntityModel<T>();

    public FilterSet<T> Filter { get; } = new();

    public IQueryable<T> AsQueryable()
    {
        return Query ?? new T[]{ }.AsQueryable();
    }

    public IReadOnlyList<IReadOnlyList<string>>? Indexes => null;

    #region IBusinessLayerService

    void IBusinessLayerService.OnPropertyChanging(object entry, string? propertyName)
    {
    }

    void IBusinessLayerService.OnPropertyChanged(object entry, string? propertyName)
    {
    }

    bool IBusinessLayerService.ValidateProperty(object entry, string propertyName, object? value, out ICollection<string>? validationErrors)
    {
        validationErrors = null;
        return true;
    }

    #endregion

    #region IEntityReadOnlyService<T>

    IFilterSet<T> IEntityReadOnlyService<T>.Filter => Filter;
    IQueryable<TNested> IEntityReadOnlyService<T>.GetNestedAsQueryable<TNested>(T entity, string? referenceName) => throw new NotSupportedException($"{nameof(EntityLinqQueryService<T>)} does not support working with nested entities.");

    #endregion

    #region IEntityService<T>

    bool IEntityService<T>.IsReadOnly => true;

    T IEntityService<T>.New()
    {
        throw new NotSupportedException();
    }

    void IEntityService<T>.Init(T entry)
    {
    }

    void IEntityService<T>.OnPropertyChanging(T entry, string? propertyName)
    {
    }

    void IEntityService<T>.OnPropertyChanged(T entry, string? propertyName)
    {
    }

    bool IEntityService<T>.ValidateProperty(T entry, string propertyName, object? value, out ICollection<string>? validationErrors)
    {
        validationErrors = null;
        return true;
    }

    IQueryable<T> IEntityService<T>.AsQueryableForUpdate()
    {
        throw new NotSupportedException();
    }

    #endregion

    #region IEntityReadOnlyService<T>

    ICollection<string>? IEntityReadOnlyService<T>.IncludeNestedData => null;

    IDataDomainScope? IEntityReadOnlyService<T>.DataDomainScope => null;

    #endregion

    #region IEntityService<T>

    void IEntityService<T>.Create(T entity)
    {
        throw new NotSupportedException();
    }

    void IEntityService<T>.Update(T entry)
    {
        throw new NotSupportedException();
    }

    void IEntityService<T>.Delete(T entity)
    {
        throw new NotSupportedException();
    }

    void IEntityService<T>.CreateRange(IEnumerable<T> entities)
    {
        throw new NotSupportedException();
    }

    void IEntityService<T>.DeleteRange(IEnumerable<T> entities)
    {
        throw new NotSupportedException();
    }

    Task IEntityService<T>.CreateAsync(T entity)
    {
        throw new NotSupportedException();
    }

    Task IEntityService<T>.UpdateAsync(T entity)
    {
        throw new NotSupportedException();
    }

    Task IEntityService<T>.DeleteAsync(T entity)
    {
        throw new NotSupportedException();
    }

    Task IEntityService<T>.CreateRangeAsync(IEnumerable<T> entities)
    {
        throw new NotSupportedException();
    }

    Task IEntityService<T>.DeleteRangeAsync(IEnumerable<T> entities)
    {
        throw new NotSupportedException();
    }

    #endregion
}