using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;
using Kapok.Entity.Model;

namespace Kapok.View;

/// <summary>
/// A view definition how the value of a specific property shall be displayed in the UI.
/// </summary>
public class PropertyView
{
    private string? _propertyName;
    private PropertyInfo? _propertyInfo;

    protected PropertyView()
    {
    }

    /// <summary>
    /// Constructs the <see cref="PropertyView"/> class based on a PropertyInfo.
    /// </summary>
    /// <param name="propertyInfo">
    /// The base property info for the PropertyView.
    /// </param>
    /// <param name="cultureInfo">
    /// The culture info used for the display attributes if set. If not set, <c>CultureInfo.CurrentUICulture</c> is used.
    /// </param>
    public PropertyView(PropertyInfo propertyInfo, CultureInfo? cultureInfo = null)
    {
        _propertyInfo = propertyInfo;

        DiscoverDisplayPropertiesFromPropertyAttributes(cultureInfo);
    }

    /// <summary>
    /// Constructs the <see cref="PropertyView"/> with just the property name.
    ///
    /// This is useful in case the type is not known at construction time. It must
    /// be set afterwards via property <see cref="DeclaringType"/> before it is
    /// used in the view.
    /// </summary>
    /// <param name="propertyName"></param>
    public PropertyView(string propertyName)
    {
        _propertyName = propertyName;
    }

    /// <summary>
    /// Auto discover the display properties for the property view
    /// from attributes of the property info. 
    /// </summary>
    protected virtual void DiscoverDisplayPropertiesFromPropertyAttributes(CultureInfo? cultureInfo = null)
    {
        Debug.Assert(PropertyInfo != null);

        cultureInfo ??= CultureInfo.CurrentUICulture;

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

    /// <summary>
    /// The declaring type of the property view.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Is thrown when setting the declaring type and a property with name <see cref="Name"/> is not found in the type definition.  
    /// </exception>
    [JsonIgnore]
    public Type? DeclaringType
    {
        get => _propertyInfo?.DeclaringType;
        set
        {
            if (value == null)
            {
                if (_propertyInfo != null)
                    _propertyName = _propertyInfo.Name;
                _propertyInfo = null;
                return;
            }

            var newPropertyInfo = value.GetProperty(Name);
            if (newPropertyInfo == null)
                throw new InvalidOperationException($"Could not find property {Name} in declaring type {DeclaringType}.");

            _propertyInfo = newPropertyInfo;
            DiscoverDisplayPropertiesFromPropertyAttributes();
        }
    }

    /// <summary>
    /// The name of the property.
    /// </summary>
#pragma warning disable CS8603
    public string Name
    {
        get { return _propertyInfo?.Name ?? _propertyName; }
        init
        {
            _propertyName = value;
        }
    }
#pragma warning restore CS8603

    [JsonIgnore]
    public PropertyInfo? PropertyInfo
    {
        get => _propertyInfo;
        set
        {
            _propertyInfo = value;
            if (value != null)
                _propertyName = value.Name;
        }
    }

    [DefaultValue(false)]
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