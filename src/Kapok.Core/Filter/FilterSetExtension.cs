using System.Globalization;
using System.Linq.Expressions;
using Kapok.Core.FilterParsing;

namespace Kapok.Core;

public static class FilterSetExtension
{
    public static object? GetDataPartitionValue(this IFilterSet filterSet, DataPartition dataPartition)
    {
        if (!filterSet.Layers.ContainsKey(FilterLayer.System))
            return null;

        var visitor = new FilterExpressionModifier(FilterExpressionModifierAction.GetFilterValue, null, dataPartition.PartitionProperty);
        visitor.Visit(filterSet.Layers[FilterLayer.System].FilterExpression);

        if (!visitor.FoundFilter)
            return null;

        return visitor.ParameterValue;
    }

    public static void SetDataPartitionValue<T>(this IFilterSet<T> filterSet, DataPartition dataScope, object? value)
        where T: class
    {
        var visitor = new FilterExpressionModifier(
            action: value == null ? FilterExpressionModifierAction.RemoveFilter : FilterExpressionModifierAction.SetFilterValue,
            baseParameterType: typeof(T),
            parameterPropertyInfo: dataScope.PartitionProperty);
        visitor.ParameterValue = value;
        if (filterSet.Layers.ContainsKey(FilterLayer.System))
        {
            var newFilterExpression = (Expression<Func<T, bool>>?)visitor.Visit(filterSet.Layers[FilterLayer.System].FilterExpression);

            if (filterSet.Layers[FilterLayer.System] is Filter<T> systemFilter)
            {
                systemFilter.FilterExpression = newFilterExpression;
            }
            else
            {
                throw new NotSupportedException("The filter for the filter layer System is of a type which can not be changed.");
            }
        }

        if (!visitor.FoundFilter && visitor.Action == FilterExpressionModifierAction.SetFilterValue)
            // NOTE: this is currently not possible because 1. we don't have access to the parameter when the expression is completely zero and 2.the IFilterSet<T> does not expose the Add(..) method today. 
            throw new NotSupportedException(
                $"An assignment to the field {dataScope.PartitionProperty.Name} for interface {dataScope.InterfaceType.FullName} is not defined. An expression can not be added!");
    }

    public static void AddPropertyFilterString<TEntry>(this IFilterSet<TEntry> filterSet,
        string propertyName,
        string? filterString, FilterLayer layer = FilterLayer.User)
        where TEntry : class
    {
        var propertyFilter = new PropertyFilterStringFilter<TEntry>(propertyName);
        propertyFilter.FilterString = filterString;
            
        filterSet.Add(propertyFilter, layer);
    }

    public static void AddPropertyFilter<TEntry, TValue>(this IFilterSet<TEntry> filterSet,
        string propertyName,
        Func<TValue> filterFuncValue, FilterLayer layer = FilterLayer.Application)
        where TEntry : class
    {
        var property = typeof(TEntry).GetProperty(propertyName);
        if (property == null)
            throw new NotSupportedException($"Could not find property {propertyName} in type {typeof(TEntry).FullName}");

        var parameter = Expression.Parameter(typeof(TEntry));
        var expression = (Expression<Func<TEntry, bool>>)Expression.Lambda(
            Expression.Equal(
                Expression.Property(parameter, property),
                Expression.Invoke(
                    Expression.Constant(filterFuncValue)
                )
            ),
            new[] {parameter}
        );

        filterSet.Add(expression, layer);
    }

    public static void AddPropertyFilter<TEntry, TValue>(this IFilterSet<TEntry> filterSet,
        string propertyName,
        TValue filterValue, FilterLayer layer = FilterLayer.User)
        where TEntry : class
    {
        var property = typeof(TEntry).GetProperty(propertyName);
        if (property == null)
            throw new NotSupportedException($"Could not find property {propertyName} in type {typeof(TEntry).FullName}");

        var propertyFilter = new PropertyStaticFilter<TEntry>(property);
        propertyFilter.FilterValue = filterValue;

        filterSet.Add(propertyFilter, layer);
    }

    public static object? GetPropertyFilterAsStaticValue<TEntry>(this IFilterSet<TEntry> filterSet,
        string propertyName, FilterLayer? layer = null)
        where TEntry : class
    {
        var property = typeof(TEntry).GetProperty(propertyName);
        if (property == null)
            throw new NotSupportedException($"Could not find property {propertyName} in type {typeof(TEntry).FullName}");

        object? GetFromFilterLayer(IPropertyFilterCollection<TEntry> propertyFilterCollection)
        {
            var propertyFilter = propertyFilterCollection.Properties
                .FirstOrDefault(p => Equals(p.PropertyInfo.Name, propertyName));
            if (propertyFilter == null)
                return null;

            if (propertyFilter is IPropertyStaticFilter<TEntry> propertyStaticFilter)
            {
                return propertyStaticFilter.FilterValue;
            }
            if (propertyFilter is IPropertyFilterStringFilter<TEntry> propertyFilterStringFilter)
            {
                return FilterExpressionParser.FilterStringToPropertyValue(
                    propertyFilterStringFilter.PropertyInfo,
                    propertyFilterStringFilter.FilterString,
                    CultureInfo.CurrentUICulture); // TODO: Should take the current UI from the ViewDomain
            }

            return null;
        }

        if (layer == null)
        {
            foreach (var filter in filterSet.Layers.Values)
            {
                if (filter is IPropertyFilterCollection<TEntry> pfc)
                {
                    object? staticValue = GetFromFilterLayer(pfc);
                    if (staticValue != null)
                        return staticValue;
                }
            }

            return null;
        }

        if (filterSet.Layers.ContainsKey(layer.Value) && filterSet.Layers[layer.Value] is IPropertyFilterCollection<TEntry> pfc2)
        {
            return GetFromFilterLayer(pfc2);
        }

        return null;
    }
}