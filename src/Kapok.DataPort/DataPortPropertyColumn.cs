using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Kapok.DataPort;

public class DataPortPropertyColumn : DataPortColumn
{
    public DataPortPropertyColumn(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
        Name = propertyInfo.Name;
        Type = propertyInfo.PropertyType;

        // TODO: we get here just one language; maybe we should load all languages here
        DisplayName = propertyInfo.GetDisplayAttributeNameOrDefault();
#pragma warning disable CS8604
        DisplayDescription = propertyInfo.GetDisplayAttributeDescriptionOrDefault();
#pragma warning restore CS8604

        var requiredAttr = propertyInfo.GetCustomAttribute<RequiredAttribute>();
        Required = requiredAttr != null;
    }

    public PropertyInfo PropertyInfo { get; set; }
}