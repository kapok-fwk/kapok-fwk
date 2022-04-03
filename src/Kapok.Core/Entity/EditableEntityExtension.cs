using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace Kapok.Entity;

public static class EditableEntityExtension
{
    public static void Map<T>(this T oldEntry, T newEntry, bool mapKey = false)
        where T : class
    {
        if (oldEntry == null) throw new ArgumentNullException(nameof(oldEntry));
        if (newEntry == null) throw new ArgumentNullException(nameof(newEntry));
        var type = oldEntry.GetType();

        var model = EntityBase.GetEntityModel<T>();
        var properties = (mapKey ? null : model.PrimaryKeyProperties) ?? Array.Empty<PropertyInfo>();

        // via reflection copy all values
        foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     // skip all primary keys
                     .Where(e => properties.All(e2 => e2.Name != e.Name)))
        {
            // ignore all fields which are not mapped on data-level
            if (Attribute.IsDefined(propertyInfo, typeof(NotMappedAttribute)))
                continue;

            if (propertyInfo.GetMethod == null || propertyInfo.SetMethod == null)
                continue;

            propertyInfo.SetMethod.Invoke(oldEntry, new[]
            {
                propertyInfo.GetMethod.Invoke(newEntry, null)
            });
        }
    }

    internal static object[] GetPrimaryKeyValues(this object entity, PropertyInfo[] primaryKeyProperties)
    {
        object[] primaryKeyValues = new object[primaryKeyProperties.Length];

        int index = 0;
        foreach (var propertyInfo in primaryKeyProperties)
        {
            if (propertyInfo.GetMethod == null)
                throw new NotSupportedException($"The primary key property {propertyInfo.Name} does not implement the Get method as public instance method.");

            primaryKeyValues[index++] = propertyInfo.GetMethod.Invoke(entity, null);
        }

        return primaryKeyValues;
    }

    public static object[] GetPrimaryKeyValues(this object entity)
    {
        var model = EntityBase.GetEntityModel(entity.GetType());
        return GetPrimaryKeyValues(entity, model.PrimaryKeyProperties);
    }

    internal static string GetPrimaryKeyAsString<T>(this T entity, PropertyInfo[] primaryKeyProperties)
        where T : class
    {
        var sb = new StringBuilder();

        foreach (var propertyInfo in primaryKeyProperties)
        {
            if (sb.Length != 0)
                sb.Append(", ");

            sb.Append(propertyInfo.Name); // TODO: use display name here?
            sb.Append(": ");

            if (propertyInfo.GetMethod == null)
                throw new NotSupportedException($"The primary key property {propertyInfo.Name} does not implement the Get method as public instance method.");

            sb.Append(propertyInfo.GetMethod.Invoke(entity, null));
        }

        return sb.ToString();
    }

    public static string GetPrimaryKeyAsString<T>(this T entity)
        where T : class
    {
        var model = EntityBase.GetEntityModel<T>();
        return GetPrimaryKeyAsString<T>(entity, model.PrimaryKeyProperties);
    }

    internal static int GetPrimaryKeyHash<T>(this T entity, PropertyInfo[] primaryKeyProperties)
    {
        if (primaryKeyProperties == null) throw new ArgumentNullException(nameof(primaryKeyProperties));

        return BuildValuesHash(
            primaryKeyProperties
                .Select(pi => pi.GetMethod.Invoke(entity, Array.Empty<object>()))
                .ToArray());
    }

    public static int GetPrimaryKeyHash<T>(this T entity)
        where T : class
    {
        var model = EntityBase.GetEntityModel<T>();
        return GetPrimaryKeyHash<T>(entity, model.PrimaryKeyProperties);
    }

    public static int BuildValuesHash(params object[] values)
    {
        unchecked
        {
            int hash = 13;
            foreach (var value in values)
            {
                hash = (hash * 7) ^
                       (value?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }
}