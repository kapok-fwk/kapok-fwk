using System.Globalization;
using Kapok.BusinessLayer.FilterParsing;

namespace Kapok.BusinessLayer;

public static class PropertyFilterExtension
{
    // ReSharper disable once MemberCanBePrivate.Global
    public static string? AsFilterString(this IPropertyStaticFilter filter, CultureInfo? cultureInfo = null)
    {
        return FilterExpressionParser.PropertyValueToFilterString(
            filter.PropertyInfo, filter.FilterValue,
            cultureInfo ?? Thread.CurrentThread.CurrentUICulture);
    }

    /// <summary>
    /// Converts the property filter to a filter string.
    /// </summary>
    /// <returns>
    /// When the filter can be converted to a filter string, the return value is the filter string.
    /// An empty string represents an empty filter.
    ///
    /// When the return value is null, the filter cannot be converted to a filter string.
    /// </returns>
    public static string? AsFilterString(this IPropertyFilter propertyFilter)
    {
        if (propertyFilter is IPropertyFilterStringFilter filterStringFilter)
        {
            return filterStringFilter.FilterString;
        }

        if (propertyFilter is IPropertyStaticFilter staticFilter)
        {
            return staticFilter.AsFilterString();
        }

        return null;
    }
}