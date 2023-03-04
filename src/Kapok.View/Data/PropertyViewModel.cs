using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Kapok.Entity;

namespace Kapok.View;

public class PropertyViewModel
{
    public PropertyViewModel(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
        {
            Name = PropertyInfo.Name;
        }
        else
        {
            var resourceManager = (System.Resources.ResourceManager?)displayAttribute.ResourceType?
                .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod
                ?.Invoke(null, null);

            if (resourceManager != null)
            {
                if (displayAttribute.Name != null)
                {
                    Name = resourceManager.GetString(displayAttribute.Name) ?? displayAttribute.Name ?? propertyInfo.Name;
                }
                else
                {
                    Name = displayAttribute.Name ?? propertyInfo.Name;
                }

                if (!string.IsNullOrEmpty(displayAttribute.Description))
                    Description = resourceManager.GetString(displayAttribute.Description) ?? displayAttribute.Description;
            }
            else
            {
                Name = displayAttribute.Name ?? propertyInfo.Name;
            }
        }
    }

    public PropertyInfo PropertyInfo { get; }

    [LookupColumn]
    public string Name { get; }

    [LookupColumn]
    public string? Description { get; }
}