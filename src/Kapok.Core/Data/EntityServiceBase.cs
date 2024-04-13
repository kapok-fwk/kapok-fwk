using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Kapok.Data;
using Kapok.Entity;
using Kapok.Entity.Model;
using Res = Kapok.Resources.Data.EntityServiceBase;

namespace Kapok.BusinessLayer;

public abstract class EntityServiceBase<T> : IEntityService<T>
    where T : class, new()
{
    protected EntityServiceBase(IDataDomainScope dataDomainScope)
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
        SetDataPartitionProperties(newEntry);
        Init(newEntry);
        return newEntry;
    }

    /// <summary>
    /// Sets the current value of the data partition to the entry.
    /// </summary>
    /// <param name="entry"></param>
    private void SetDataPartitionProperties(T entry)
    {
        foreach (var pair in DataDomainScope.DataPartitions)
        {
            var dataPartitionKey = pair.Key;
            var dataPartition = pair.Value;
            
            if (dataPartition.InterfaceType.IsAssignableFrom(typeof(T)))
            {
#pragma warning disable CS8602
                dataPartition.PartitionProperty.SetMethod.Invoke(entry,
#pragma warning restore CS8602
                    new[]
                    {
                        DataDomainScope.DataPartitions[dataPartitionKey].Value
                        // current value
                    });
            }
        }
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

            var autoGenerateValueAttribute = property.GetCustomAttribute<AutoGenerateValueAttribute>();
            if (autoGenerateValueAttribute is { Type: AutoGenerateValueType.Identity })
            {
                if (property.PropertyType == typeof(Guid))
                {
                    // Generates a new Guid
                    property.SetValue(entry, Guid.NewGuid());
                }
                else
                {
                    throw new NotSupportedException(
                        $"The {nameof(AutoGenerateValueAttribute)} is not supported with Identity for properties of type {property.PropertyType.FullName}");
                }
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

    public virtual bool ValidateProperty(T entry, string propertyName, object? value, out ICollection<string> validationErrors)
    {
        validationErrors = new List<string>();

        var propertyInfo = typeof(T).GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

        if (propertyInfo == null)
            throw new ArgumentException(string.Format(Res.ValidationProprertyNotFound, propertyName), nameof(propertyName));

        bool isValid = true;

        // do the data model validation during the server is called for the property-specific business layer validation
        var validationResults = new List<ValidationResult>();
        ValidationContext validationContext =
            new ValidationContext(entry, null, null)
            {
                MemberName = propertyName,

                DisplayName = propertyInfo.GetDisplayAttributeNameOrDefault()
            };
        var validatorResult = Validator.TryValidateProperty(value, validationContext, validationResults);

        if (!validatorResult)
        {
            foreach (var validationResult in validationResults)
            {
                validationErrors.Add(validationResult.ErrorMessage ?? string.Format(Res.ValidationError, propertyName));
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
#pragma warning disable CS8602
        return GetType().GetMethod(nameof(OnCreate), BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType != typeof(EntityServiceBase<T>);
#pragma warning restore CS8602
    }

    /// <summary>
    /// Returns true when the OnUpdate method is implement (= overriden).
    /// </summary>
    /// <returns></returns>
    protected bool IsOnUpdateImplemented()
    {
        // ReSharper disable once PossibleNullReferenceException
#pragma warning disable CS8602
        return GetType().GetMethod(nameof(OnUpdate), BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType != typeof(EntityServiceBase<T>);
#pragma warning restore CS8602
    }

    /// <summary>
    /// Returns true when the OnDelete method is implement (= overriden).
    /// </summary>
    /// <returns></returns>
    protected bool IsOnDeleteImplemented()
    {
        // ReSharper disable once PossibleNullReferenceException
#pragma warning disable CS8602
        return GetType().GetMethod(nameof(OnDelete), BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType != typeof(EntityServiceBase<T>);
#pragma warning restore CS8602
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

    #region IEntityReadOnlyService<T>

    IFilterSet<T> IEntityReadOnlyService<T>.Filter => Filter;

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

    bool IBusinessLayerService.ValidateProperty(object entry, string propertyName, object? value, out ICollection<string>? validationErrors)
    {
        return ValidateProperty((T) entry, propertyName, value, out validationErrors);
    }

    #endregion
}