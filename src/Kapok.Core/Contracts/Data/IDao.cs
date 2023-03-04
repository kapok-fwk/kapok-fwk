namespace Kapok.BusinessLayer;

public interface IDao<T> : IBusinessLayerService, IReadOnlyDao<T>
    where T : class, new()
{
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);
    void CreateRange(IEnumerable<T> entities);
    void DeleteRange(IEnumerable<T> entities);

    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task CreateRangeAsync(IEnumerable<T> entities);
    Task DeleteRangeAsync(IEnumerable<T> entities);


    bool IsReadOnly { get; }

    T New();
    void Init(T entry);
    void OnPropertyChanging(T entry, string? propertyName);
    void OnPropertyChanged(T entry, string? propertyName);
    bool ValidateProperty(T entry, string propertyName, object? value,
        out ICollection<string>? validationErrors);

    IQueryable<T> AsQueryableForUpdate();
}