using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kapok.Entity.Model;

namespace Kapok.Entity;

public abstract class EntityBase : BindableObjectBase, INotifyPropertyChanging
{
    #region static entity model
        
    private static readonly Dictionary<Type, IEntityModel> EntityModels = new();

    public static IReadOnlyDictionary<Type, IEntityModel> GetAllEntityModels()
    {
        return EntityModels;
    }

    public static IEntityModel GetEntityModel<TEntity>()
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        if (EntityModels.ContainsKey(entityType))
            return EntityModels[entityType];

        // create an empty instance to make sure that when there is a static method to register the entity, it is called now
        Activator.CreateInstance(entityType);

        // when there is still no model, we will create here an empty model
        if (!EntityModels.ContainsKey(entityType))
        {
            // create an empty model
            var entityModelBuilder = new EntityModelBuilder<TEntity>();
                
            // and add it to the cache
            EntityModels.Add(entityType, entityModelBuilder.Model);

            return entityModelBuilder.Model;
        }

        return EntityModels[entityType];
    }

    public static IEntityModel GetEntityModel(Type entityType)
    {
        if (EntityModels.ContainsKey(entityType))
            return EntityModels[entityType];

        // create an empty instance to make sure that when there is a static method to register the entity, it is called now
        Activator.CreateInstance(entityType);

        // when there is still no model, we will create here an empty model
        if (!EntityModels.ContainsKey(entityType))
        {
            // create an empty model
            var entityModelBuilderType = typeof(EntityModelBuilder<>).MakeGenericType(entityType);
            var entityModelBuilder =
#pragma warning disable CS8602
                entityModelBuilderType
                    .GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null,
                        new[] {typeof(EntityModel)}, null)
#pragma warning restore CS8602
                    .Invoke(new object?[] {null});

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8602
            var model = (IEntityModel) entityModelBuilderType
                .GetProperty(nameof(EntityModelBuilder<object>.Model),
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
#pragma warning restore CS8602
                .GetMethod.Invoke(entityModelBuilder, null);
#pragma warning restore CS8602
#pragma warning restore CS8600

            // and add it to the cache
#pragma warning disable CS8604
            EntityModels.Add(entityType, model);
#pragma warning restore CS8604
        }

        return EntityModels[entityType];
    }

    protected static void RegisterModel<TEntity>(Action<EntityModelBuilder<TEntity>>? action)
        where TEntity : class
    {
        var entityModelBuilder = new EntityModelBuilder<TEntity>();
        action?.Invoke(entityModelBuilder);
        EntityModels.Add(typeof(TEntity), entityModelBuilder.Model);
    }
        
    #endregion

    protected override bool SetProperty<T>(ref T? storage, T? value, [CallerMemberName] string? propertyName = null)
        where T : default
    {
        if (Equals(storage, value))
            return false;

        OnPropertyChanging(propertyName);
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #region INotifyPropertyChanging

    public event PropertyChangingEventHandler? PropertyChanging;

    protected virtual void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    #endregion

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;

        Type thisType = GetType();

        if (obj.GetType() != thisType)
            return false;

        PropertyInfo[]? thisEntityPrimaryKeys;
        if (!EntityModels.ContainsKey(thisType) || (thisEntityPrimaryKeys = EntityModels[thisType].PrimaryKeyProperties) == null)
            return base.GetHashCode() == obj.GetHashCode();

        foreach (var propertyInfo in thisEntityPrimaryKeys)
        {
            var getter = propertyInfo.GetMethod;

#pragma warning disable CS8602
            var value1 = getter.Invoke(this, null);
#pragma warning restore CS8602
            var value2 = getter.Invoke(obj, null);

            if (value1 == null)
            {
                if (value2 != null)
                    return false;
            }
#pragma warning disable CS8602
            else if (!getter.Invoke(this, null).Equals(getter.Invoke(obj, null)))
#pragma warning restore CS8602
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        Type thisType = GetType();

        PropertyInfo[]? thisEntityPrimaryKeys;
        if (!EntityModels.ContainsKey(thisType) || (thisEntityPrimaryKeys = EntityModels[thisType].PrimaryKeyProperties) == null)
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();

        unchecked
        {
            int hash = 13;
            foreach (var propertyInfo in thisEntityPrimaryKeys)
            {
                var value = propertyInfo.GetMethod?.Invoke(this, null);

                hash = (hash * 7) ^
                       (value?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }
}