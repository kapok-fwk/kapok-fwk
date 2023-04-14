using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Resources;
using Kapok.Resources.Entity;

// ReSharper disable once CheckNamespace
namespace System.Reflection;

public static class PropertyInfoExtensions
{
    /// <summary>
    /// The internal property name in a resource to linking to the resource manager.
    /// </summary>
    private const string ResourceTypeResourceManagerPropertyName = "ResourceManager";

    /// <summary>
    /// Returns the resource manager of a resource type class object.
    /// </summary>
    /// <param name="resourceType"></param>
    /// <returns></returns>
    private static ResourceManager? GetResourceManager(Type? resourceType)
    {
        return (ResourceManager?)resourceType
            ?.GetProperty(ResourceTypeResourceManagerPropertyName, BindingFlags.Public | BindingFlags.Static)
            ?.GetMethod?.Invoke(null, null);
    }

    public static string GetDisplayAttributeNameOrDefault(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute?.Name == null)
            return propertyInfo.Name;

        return GetResourceManager(displayAttribute.ResourceType)?
                   .GetString(displayAttribute.Name) ?? 
               displayAttribute.Name;
    }

    public static string GetDisplayAttributeName(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            throw new NotSupportedException($"The property {propertyInfo.Name} does not implement attribute {nameof(DisplayAttribute)} which is required for the extension method {nameof(GetDisplayAttributeName)}.");

        if (displayAttribute.Name == null)
            return propertyInfo.Name;

        return GetResourceManager(displayAttribute.ResourceType)?
                   .GetString(displayAttribute.Name) ??
               displayAttribute.Name;
    }

    public static string GetDisplayAttributeNameOrDefault(this PropertyInfo propertyInfo, CultureInfo? cultureInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute?.Name == null)
            return propertyInfo.Name;

        var resourceManager = GetResourceManager(displayAttribute.ResourceType);

        return resourceManager?.GetString(displayAttribute.Name, cultureInfo) ??
               resourceManager?.GetString(displayAttribute.Name) ??
               displayAttribute.Name;
    }

    public static string GetDisplayAttributeName(this PropertyInfo propertyInfo, CultureInfo? cultureInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            throw new NotSupportedException($"The property {propertyInfo.Name} does not implement attribute {nameof(DisplayAttribute)} which is required for the extension method {nameof(GetDisplayAttributeName)}.");

        if (displayAttribute.Name == null)
            return propertyInfo.Name;

        var resourceManager = GetResourceManager(displayAttribute.ResourceType);

        return resourceManager?.GetString(displayAttribute.Name, cultureInfo) ??
               resourceManager?.GetString(displayAttribute.Name) ??
               displayAttribute.Name;
    }

    public static string? GetDisplayAttributeDescriptionOrDefault(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute?.Description == null)
            return null;

        return GetResourceManager(displayAttribute.ResourceType)?
                   .GetString(displayAttribute.Description) ??
               displayAttribute.Description;
    }

    public static string? GetDisplayAttributeDescription(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            throw new NotSupportedException($"The property {propertyInfo.Name} does not implement attribute {nameof(DisplayAttribute)} which is required for the extension method {nameof(GetDisplayAttributeName)}.");

        if (displayAttribute.Description == null)
            return null;

        return GetResourceManager(displayAttribute.ResourceType)?
                   .GetString(displayAttribute.Description) ??
               displayAttribute.Description;
    }

    public static string? GetDisplayAttributeDescriptionOrDefault(this PropertyInfo propertyInfo, CultureInfo? cultureInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute?.Description == null)
            return null;

        var resourceManager = GetResourceManager(displayAttribute.ResourceType);

        return resourceManager?.GetString(displayAttribute.Description, cultureInfo) ??
               resourceManager?.GetString(displayAttribute.Description) ??
               displayAttribute.Description;
    }

    public static string? GetDisplayAttributeDescription(this PropertyInfo propertyInfo, CultureInfo? cultureInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            throw new NotSupportedException($"The property {propertyInfo.Name} does not implement attribute {nameof(DisplayAttribute)} which is required for the extension method {nameof(GetDisplayAttributeName)}.");

        if (displayAttribute.Description == null)
            return null;

        var resourceManager = GetResourceManager(displayAttribute.ResourceType);

        return resourceManager?.GetString(displayAttribute.Description, cultureInfo) ??
               resourceManager?.GetString(displayAttribute.Description) ??
               displayAttribute.Description;
    }

    public static int MaxStringLength(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));

        var attributes = propertyInfo.GetCustomAttributes(typeof(StringLengthAttribute), true);
        if (attributes == null || attributes.Length == 0 || !(attributes[0] is StringLengthAttribute stringLengthAttribute))
            throw new NotSupportedException(string.Format(EntityExtension.Property_AttributeDoesNotExist, nameof(propertyInfo.Name), nameof(StringLengthAttribute)));

        return stringLengthAttribute.MaximumLength;
    }

    public static int MinStringLength(this PropertyInfo? propertyInfo)
    {
        if (propertyInfo == null)
            return 0;

        var attributes = propertyInfo.GetCustomAttributes(typeof(StringLengthAttribute), true);
        if (attributes == null || attributes.Length == 0 || !(attributes[0] is StringLengthAttribute stringLengthAttribute))
            throw new NotSupportedException(string.Format(EntityExtension.Property_AttributeDoesNotExist, nameof(propertyInfo.Name), nameof(StringLengthAttribute)));

        return stringLengthAttribute.MinimumLength;
    }
}