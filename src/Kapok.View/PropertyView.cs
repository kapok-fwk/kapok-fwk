using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Kapok.Entity.Model;

namespace Kapok.View;

/// <summary>
/// Defines a property view.
/// </summary>
public class PropertyView
{
    public PropertyView(PropertyInfo propertyInfo, CultureInfo? cultureInfo = null)
    {
        PropertyInfo = propertyInfo;

        if (cultureInfo == null)
            cultureInfo = CultureInfo.CurrentUICulture;

        // initialize properties from display attributes:

        var displayAttribute = PropertyInfo.GetCustomAttribute<DisplayAttribute>();
        if (displayAttribute != null)
        {
            System.Resources.ResourceManager? resourceManager =
                (System.Resources.ResourceManager?)displayAttribute.ResourceType?
                    .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
                    .Invoke(null, null);

            if (!string.IsNullOrEmpty(displayAttribute.Name))
            {
                var name = resourceManager?.GetString(displayAttribute.Name, cultureInfo) ?? displayAttribute.Name;

                DisplayName = name;
            }

            if (!string.IsNullOrEmpty(displayAttribute.ShortName))
            {
                var shortName = resourceManager?.GetString(displayAttribute.ShortName, cultureInfo) ??
                                displayAttribute.ShortName;

                DisplayShortName = shortName;

                if (DisplayName == null)
                    DisplayName = DisplayShortName;
            }

            if (!string.IsNullOrEmpty(displayAttribute.Description))
            {
                var description = resourceManager?.GetString(displayAttribute.Description, cultureInfo) ??
                                  displayAttribute.Description;

                DisplayDescription = description;
            }
        }

        var dataTypeAttribute = PropertyInfo.GetCustomAttribute<DataTypeAttribute>();
        var displayFormatAttribute = PropertyInfo.GetCustomAttribute<DisplayFormatAttribute>();
        if (displayFormatAttribute != null)
        {
            StringFormat = displayFormatAttribute.DataFormatString;

            if (displayFormatAttribute.NullDisplayText != null)
            {
                var nullDisplayText = displayFormatAttribute.NullDisplayText;
                if (displayAttribute != null)
                {
                    // will try to apply the text with the resource given from the DisplayAttribute
                    System.Resources.ResourceManager? resourceManager =
                        (System.Resources.ResourceManager?)displayAttribute.ResourceType?
                            .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)
                            ?.GetMethod?
                            .Invoke(null, null);

                    var resourceValue = resourceManager?.GetString(nullDisplayText);
                    if (resourceValue != null)
                        nullDisplayText = resourceValue;
                }

                NullDisplayText = nullDisplayText;
            }
        }
        else if (dataTypeAttribute != null)
        {
            switch (dataTypeAttribute.DataType)
            {
                case DataType.Date:
                    StringFormat = "d";
                    break;
            }
        }
    }

    public PropertyInfo PropertyInfo { get; set; }

    public bool IsReadOnly { get; set; }

    public int? ArrayIndex { get; set; }

    public ILookupDefinition? LookupDefinition { get; set; }

    public IDrillDownDefinition? DrillDownDefinition { get; set; }

    #region Display members

    /// <summary>
    /// The name which is displayed in the UI.
    ///
    /// This name will be used preferably in table columns. By some UIs in
    /// general, in some UIs only when the visible space for
    /// 'DisplayName' is not enough.
    /// </summary>
    public Caption? DisplayShortName { get; set; }

    /// <summary>
    /// The name which is displayed in the UI.
    /// </summary>
    public Caption? DisplayName { get; set; }

    /// <summary>
    /// The description which is displayed for the column.
    /// </summary>
    public Caption? DisplayDescription { get; set; }

    public string? StringFormat { get; set; }
    public string? NullDisplayText { get; set; }

    #endregion
}