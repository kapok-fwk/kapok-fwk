using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Kapok.Entity;
using Kapok.Entity.Model;

namespace Kapok.Core;

public abstract class DaoBase<T> : IDao<T>
    where T : class, new()
{
    protected DaoBase(IDataDomainScope dataDomainScope)
    {
        DataDomainScope = dataDomainScope;
    }

    public IDataDomainScope DataDomainScope { get; }

    public IEntityModel Model => EntityBase.GetEntityModel<T>();

    public FilterSet<T> Filter { get; } = new();

    public ICollection<string> IncludeNestedData { get; } = new List<string>();

    public abstract bool IsReadOnly { get; protected set; }

    public virtual T New()
    {
        var newEntry = new T();
        if (typeof(T).IsSubclassOf(typeof(EditableEntityBase)))
        {
            if (newEntry is EditableEntityBase newEditableEntity)
                EditableEntityBase.SetBusinessLayerService(newEditableEntity, this);
        }
        Init(newEntry);
        return newEntry;
    }
        
    public abstract void Create(T entry);
    public abstract void Update(T entry);
    public abstract void Delete(T entry);
    public abstract void CreateRange(IEnumerable<T> entries);
    public abstract void DeleteRange(IEnumerable<T> entries);
    public abstract Task CreateAsync(T entity);
    public abstract Task UpdateAsync(T entity);
    public abstract Task DeleteAsync(T entity);
    public abstract Task CreateRangeAsync(IEnumerable<T> entities);
    public abstract Task DeleteRangeAsync(IEnumerable<T> entities);

    public virtual void Init(T entry)
    {
        // set the default value from the [DefaultValue(..)] attribute
        var potentialProperties =
            typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
        foreach (var property in potentialProperties)
        {
            var defaultValueAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttribute != null)
            {
                property.SetValue(entry, defaultValueAttribute.Value);
            }
        }
    }

    public abstract IQueryable<T> AsQueryable();
    public abstract IQueryable<T> AsQueryableForUpdate();
    public abstract IQueryable<TNested> GetNestedAsQueryable<TNested>(T entity, string? referenceName = null)
        where TNested : class, new();

    public virtual void OnPropertyChanging(T entry, string? propertyName)
    {
    }

    public virtual void OnPropertyChanged(T entry, string? propertyName)
    {
    }

    public virtual bool ValidateProperty(T entry, string? propertyName, object? value, out ICollection<string> validationErrors)
    {
        validationErrors = new List<string>();

        var propertyInfo = typeof(T).GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

        if (propertyInfo == null)
            throw new ArgumentException($"Could not find property with name {propertyName}.", nameof(propertyName));

        bool isValid = true;

        // do the data model validation during the server is called for the property-specific business layer validation
        var validationResults = new List<ValidationResult>();
        ValidationContext validationContext =
            new ValidationContext(entry, null, null)
            {
                MemberName = propertyName,

                // TODO: here we should get the client culture, not just use the server culture information
                DisplayName = propertyInfo.GetDisplayAttributeNameOrDefault() ?? propertyName
            };
        var validatorResult = Validator.TryValidateProperty(value, validationContext, validationResults);

        if (!validatorResult)
        {
            foreach (var validationResult in validationResults)
            {
                validationErrors.Add(validationResult.ErrorMessage ?? $"Validation error for property '{propertyName}'");
            }

            isValid = validationErrors.Count == 0;
        }

        return isValid;
    }

    #region Internal overrideable members

    /// <summary>
    /// Returns true when the OnCreate method is implement (= overriden).
    /// </summary>
    /// <returns></returns>
    protected bool IsOnCreateImplemented()
    {
        // ReSharper disable once PossibleNullReferenceException
        return GetType().GetMethod(nameof(OnCreate), BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType != typeof(DaoBase<T>);
    }

    /// <summary>
    /// Returns true when the OnUpdate method is implement (= overriden).
    /// </summary>
    /// <returns></returns>
    protected bool IsOnUpdateImplemented()
    {
        // ReSharper disable once PossibleNullReferenceException
        return GetType().GetMethod(nameof(OnUpdate), BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType != typeof(DaoBase<T>);
    }

    /// <summary>
    /// Returns true when the OnDelete method is implement (= overriden).
    /// </summary>
    /// <returns></returns>
    protected bool IsOnDeleteImplemented()
    {
        // ReSharper disable once PossibleNullReferenceException
        return GetType().GetMethod(nameof(OnDelete), BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType != typeof(DaoBase<T>);
    }

    protected virtual async Task OnCreate(ICollection<T> entries, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    protected virtual async Task OnUpdate(ICollection<T> entries, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    protected virtual async Task OnDelete(ICollection<T> entries, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    #endregion

    #region IReadOnlyDao<T>

    IFilterSet<T> IReadOnlyDao<T>.Filter => Filter;

    #endregion

    #region IBusinessLayerService

    void IBusinessLayerService.OnPropertyChanging(object entry, string? propertyName)
    {
        OnPropertyChanging((T)entry, propertyName);
    }

    void IBusinessLayerService.OnPropertyChanged(object entry, string? propertyName)
    {
        OnPropertyChanged((T)entry, propertyName);
    }

    bool IBusinessLayerService.ValidateProperty(object entry, string? propertyName, object? value, out ICollection<string> validationErrors)
    {
        return ValidateProperty((T) entry, propertyName, value, out validationErrors);
    }

    #endregion
}