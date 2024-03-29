﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Kapok.Entity;

namespace Kapok.View;

/// <summary>
/// The view definition for a column in a table.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class ColumnPropertyView : PropertyView
{
    public ColumnPropertyView()
    {
    }

    public ColumnPropertyView(PropertyInfo propertyInfo) : base(propertyInfo)
    {
    }

    public ColumnPropertyView(string propertyName) : base(propertyName)
    {
    }

    protected override void DiscoverDisplayPropertiesFromPropertyAttributes(CultureInfo? cultureInfo = null)
    {
        Debug.Assert(PropertyInfo != null);
        base.DiscoverDisplayPropertiesFromPropertyAttributes(cultureInfo);
        
        if (PropertyInfo.SetMethod == null || (PropertyInfo.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly ?? false))
            IsReadOnly = true;

        var autoCalculateAttribute = PropertyInfo.GetCustomAttribute<AutoCalculateAttribute>();
        if (autoCalculateAttribute != null)
            IsReadOnly = true;
    }

    public double? Width { get; set; }

    /// <summary>
    /// Gives back if the property view can be filtered in the view.
    /// </summary>
    [DefaultValue(true)]
    public bool IsFilterable
    {
        get
        {
            if (PropertyInfo == null)
                return true;  // by default we assume a field is filterable.

            // The attribute 'NotMapped' says that the data is not stored in the database.
            // Since next to all repositories are liked to the database (except you use InMemory repositories)
            // we disable this here in general.
            //
            // In the future it might be interesting to implement this for InMemory as well. But this
            // is not done here because if the repository is in-memory or not is not known in this code scope.
            //
            // For 'flow fields' / calculated fields which shall be filtered we could maybe implement here
            // something special at some day ...
            if (Attribute.IsDefined(PropertyInfo, typeof(NotMappedAttribute)))
                return false;

            if (Attribute.IsDefined(PropertyInfo, typeof(InfoImagesAttribute)))
                return false;
                
            return true;
        }
    }

    /// <summary>
    /// Indicates that this column field is hidden from the user view.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [DefaultValue(false)]
    public bool IsHidden { get; set; }

    /// <summary>
    /// Shows a hierarchical tree at this column.
    ///
    /// This requires that the entity implements IHierarchyEntry`1.
    /// </summary>
    [DefaultValue(false)]
    public bool ShowHierarchicalTree { get; set; }

    /// <summary>
    /// If the text shall be wrapped to a second line if longer than <see cref="Width"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool TextWrap { get; set; } = false;
}