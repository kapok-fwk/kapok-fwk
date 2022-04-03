using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Kapok.Core.Resources.Entity;

namespace System.Reflection;

public static class PropertyInfoExtensions
{
    public static string GetDisplayAttributeNameOrDefault(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            return propertyInfo.Name;

        var resourceManager = (Resources.ResourceManager?)displayAttribute.ResourceType?
            .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
            .Invoke(null, null);

        if (displayAttribute.Name == null)
            return propertyInfo.Name;

        return resourceManager?.GetString(displayAttribute.Name) ??
               displayAttribute.Name;
    }

    public static string GetDisplayAttributeName(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            throw new NotSupportedException($"The property {propertyInfo.Name} does not implement attribute {nameof(DisplayAttribute)} which is required for the extension method {nameof(GetDisplayAttributeName)}.");

        var resourceManager = (Resources.ResourceManager?)displayAttribute.ResourceType?
            .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
            .Invoke(null, null);

        if (displayAttribute.Name == null)
            return propertyInfo.Name;

        return resourceManager?.GetString(displayAttribute.Name) ??
               displayAttribute.Name;
    }

    public static string GetDisplayAttributeNameOrDefault(this PropertyInfo propertyInfo, CultureInfo cultureInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            return propertyInfo.Name;

        var resourceManager = (Resources.ResourceManager?)displayAttribute.ResourceType?
            .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
            .Invoke(null, null);

        if (displayAttribute.Name == null)
            return propertyInfo.Name;

        return resourceManager?.GetString(displayAttribute.Name, cultureInfo) ??
               resourceManager?.GetString(displayAttribute.Name) ??
               displayAttribute.Name;
    }

    public static string GetDisplayAttributeName(this PropertyInfo propertyInfo, CultureInfo cultureInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            throw new NotSupportedException($"The property {propertyInfo.Name} does not implement attribute {nameof(DisplayAttribute)} which is required for the extension method {nameof(GetDisplayAttributeName)}.");

        var resourceManager = (Resources.ResourceManager?)displayAttribute.ResourceType?
            .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
            .Invoke(null, null);

        if (displayAttribute.Name == null)
            return propertyInfo.Name;

        return resourceManager?.GetString(displayAttribute.Name, cultureInfo) ??
               resourceManager?.GetString(displayAttribute.Name) ??
               displayAttribute.Name;
    }

    public static string? GetDisplayAttributeDescriptionOrDefault(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            return null;

        var resourceManager = (Resources.ResourceManager?)displayAttribute.ResourceType?
            .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
            .Invoke(null, null);

        if (displayAttribute.Description == null)
            return null;

        return resourceManager?.GetString(displayAttribute.Description) ??
               displayAttribute.Description;
    }

    public static string GetDisplayAttributeDescription(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            throw new NotSupportedException($"The property {propertyInfo.Name} does not implement attribute {nameof(DisplayAttribute)} which is required for the extension method {nameof(GetDisplayAttributeName)}.");

        var resourceManager = (Resources.ResourceManager?)displayAttribute.ResourceType?
            .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
            .Invoke(null, null);

        if (displayAttribute.Description == null)
            return string.Empty;

        return resourceManager?.GetString(displayAttribute.Description) ??
               displayAttribute.Description;
    }

    public static string? GetDisplayAttributeDescriptionOrDefault(this PropertyInfo propertyInfo, CultureInfo cultureInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            return null;

        var resourceManager = (Resources.ResourceManager?)displayAttribute.ResourceType?
            .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
            .Invoke(null, null);

        if (displayAttribute.Description == null)
            return null;

        return resourceManager?.GetString(displayAttribute.Description, cultureInfo) ??
               resourceManager?.GetString(displayAttribute.Description) ??
               displayAttribute.Description;
    }

    public static string GetDisplayAttributeDescription(this PropertyInfo propertyInfo, CultureInfo cultureInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            throw new NotSupportedException($"The property {propertyInfo.Name} does not implement attribute {nameof(DisplayAttribute)} which is required for the extension method {nameof(GetDisplayAttributeName)}.");

        var resourceManager = (Resources.ResourceManager?)displayAttribute.ResourceType?
            .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
            .Invoke(null, null);

        if (displayAttribute.Description == null)
            return string.Empty;

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