using System.Diagnostics;
using System.Globalization;
using Kapok.Core;
using Kapok.Core.FilterParsing;
using Kapok.Entity;

namespace Kapok.View;

public static class NestedDataFilterExtension
{
    /// <summary>
    /// Get all filter values to be used for an nested data filter.
    /// </summary>
    /// <param name="filterSet"></param>
    /// <param name="viewDomain"></param>
    /// <returns></returns>
    public static IReadOnlyDictionary<string, object> GetNestedDataFilter(this IFilterSet filterSet, IViewDomain? viewDomain)
    {
        var nestedDataFilter = new Dictionary<string, object>();
        foreach (var filterList in filterSet.Layers.Values)
        {
            if (!(filterList is IPropertyFilterCollection propertyFilterCollection))
                continue;

            foreach (var propertyFilter in propertyFilterCollection.Properties)
            {
                if (!Attribute.IsDefined(propertyFilter.PropertyInfo, typeof(NestedDataFilterAttribute)))
                    continue; 

                if (propertyFilter is IPropertyStaticFilter propertyStaticFilter)
                {
                    nestedDataFilter.Add(propertyFilter.PropertyInfo.Name, propertyStaticFilter.FilterValue);
                }
                else if (propertyFilter is IPropertyFilterStringFilter propertyStringFilter)
                {
                    try
                    {
                        object filterValue = FilterExpressionParser.FilterStringToPropertyValue(
                            propertyStringFilter.PropertyInfo,
                            propertyStringFilter.FilterString,
                            viewDomain?.Culture ?? CultureInfo.InvariantCulture);

                        nestedDataFilter.Add(propertyFilter.PropertyInfo.Name, filterValue);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error while parsing nested data filter string for property {propertyFilter.PropertyInfo.Name}: {e.Message}");
                    }
                }
            }
        }

        return nestedDataFilter;
    }
}