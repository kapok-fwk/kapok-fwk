using Kapok.Data;
using Kapok.Entity;
using Kapok.Entity.Model;

namespace Kapok.BusinessLayer;

public class LinqQueryDao<T> : IDao<T>
    where T : class, new()
{
    public IQueryable<T>? Query { get; set; }

    public LinqQueryDao(IQueryable<T>? query = null)
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

    #region IReadOnlyDao<T>

    IFilterSet<T> IReadOnlyDao<T>.Filter => Filter;
    IQueryable<TNested> IReadOnlyDao<T>.GetNestedAsQueryable<TNested>(T entity, string? referenceName) => throw new NotSupportedException($"{nameof(LinqQueryDao<T>)} does not support working with nested entities.");

    #endregion

    #region IDao<T>

    bool IDao<T>.IsReadOnly => true;

    T IDao<T>.New()
    {
        throw new NotSupportedException();
    }

    void IDao<T>.Init(T entry)
    {
    }

    void IDao<T>.OnPropertyChanging(T entry, string? propertyName)
    {
    }

    void IDao<T>.OnPropertyChanged(T entry, string? propertyName)
    {
    }

    bool IDao<T>.ValidateProperty(T entry, string propertyName, object? value, out ICollection<string>? validationErrors)
    {
        validationErrors = null;
        return true;
    }

    IQueryable<T> IDao<T>.AsQueryableForUpdate()
    {
        throw new NotSupportedException();
    }

    #endregion

    #region IReadOnlyDao<T>

    ICollection<string>? IReadOnlyDao<T>.IncludeNestedData => null;

    IDataDomainScope? IReadOnlyDao<T>.DataDomainScope => null;

    #endregion

    #region IDao<T>
        
    void IDao<T>.Create(T entity)
    {
        throw new NotSupportedException();
    }

    void IDao<T>.Update(T entry)
    {
        throw new NotSupportedException();
    }

    void IDao<T>.Delete(T entity)
    {
        throw new NotSupportedException();
    }

    void IDao<T>.CreateRange(IEnumerable<T> entities)
    {
        throw new NotSupportedException();
    }

    void IDao<T>.DeleteRange(IEnumerable<T> entities)
    {
        throw new NotSupportedException();
    }

    Task IDao<T>.CreateAsync(T entity)
    {
        throw new NotSupportedException();
    }

    Task IDao<T>.UpdateAsync(T entity)
    {
        throw new NotSupportedException();
    }

    Task IDao<T>.DeleteAsync(T entity)
    {
        throw new NotSupportedException();
    }

    Task IDao<T>.CreateRangeAsync(IEnumerable<T> entities)
    {
        throw new NotSupportedException();
    }

    Task IDao<T>.DeleteRangeAsync(IEnumerable<T> entities)
    {
        throw new NotSupportedException();
    }

    #endregion
}