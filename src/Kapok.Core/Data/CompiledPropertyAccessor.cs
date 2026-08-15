using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Kapok.Data;

/// <summary>
/// Caches compiled getter/setter delegates per <see cref="PropertyInfo"/>, so that repeated property
/// access (e.g. entity cloning/diffing in <see cref="DtoMapper"/>) does not pay the cost of
/// <see cref="PropertyInfo"/>.Invoke on every single call.
/// </summary>
internal static class CompiledPropertyAccessor
{
    private static readonly ConcurrentDictionary<PropertyInfo, Func<object, object?>> Getters = new();
    private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object?>> Setters = new();

    public static object? GetValue(PropertyInfo property, object instance)
        => Getters.GetOrAdd(property, CreateGetter)(instance);

    public static void SetValue(PropertyInfo property, object instance, object? value)
        => Setters.GetOrAdd(property, CreateSetter)(instance, value);

    private static Func<object, object?> CreateGetter(PropertyInfo property)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var typedInstance = Expression.Convert(instanceParam, property.DeclaringType!);
        var propertyAccess = Expression.Property(typedInstance, property);
        var typedResult = Expression.Convert(propertyAccess, typeof(object));

        return Expression.Lambda<Func<object, object?>>(typedResult, instanceParam).Compile();
    }

    private static Action<object, object?> CreateSetter(PropertyInfo property)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var valueParam = Expression.Parameter(typeof(object), "value");
        var typedInstance = Expression.Convert(instanceParam, property.DeclaringType!);
        var typedValue = Expression.Convert(valueParam, property.PropertyType);
        var propertyAccess = Expression.Property(typedInstance, property);
        var assign = Expression.Assign(propertyAccess, typedValue);

        return Expression.Lambda<Action<object, object?>>(assign, instanceParam, valueParam).Compile();
    }
}
