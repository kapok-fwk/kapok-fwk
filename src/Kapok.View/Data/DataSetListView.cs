using System.Reflection;

namespace Kapok.View;

/// <summary>
/// A class storing information about how to show a DataSet in a UI list.
/// </summary>
public class DataSetListView : IDataSetListView
{
    private Caption? _displayName;

    /// <summary>
    /// Internal name of the list view.
    ///
    /// This name is used to identify internal meta data for a list view
    /// (e.g. a custom sorting for a list for a specific user/user group).
    ///
    /// It is used as a fallback when property <c>DisplayName</c> is not defined. It is
    /// recommended to use <c>DisplayName</c> for a display name for a list view instead
    /// of this property because <c>DisplayName</c> supports localization.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Name shown to the user for the list view.
    /// </summary>
    public Caption? DisplayName
    {
        get => _displayName ?? new Caption { { Caption.EmptyLanguage, Name ?? ToString() } };
        set => _displayName = value;
    }

    /// <summary>
    /// INTERNAL. Used in WPF as command to select the DataSetListView as current ListView of e.g. a ListPage&lt;T&gt;.
    /// </summary>
    // not nice; necessary for toolbar menu select in WPF
    public IAction? SelectAction { get; set; }

    /// <summary>
    /// The columns which shall be shown in the UI list.
    /// </summary>
    public List<ColumnPropertyView>? Columns { get; set; }

    /// <summary>
    /// The default sorting of the list. If not set, the primary key is used.
    /// When the entity has no primary key, no sorting is enforced which
    /// leaves the decision of the entity order to the data domain.
    ///
    /// In case of a data domain connected to a relational database the sorting
    /// could be randomly selected.
    /// </summary>
    public PropertyInfo[]? SortBy { get; set; }

    /// <summary>
    /// The default sort direction of the list.
    ///
    /// When the <c>SortBy</c> property is defined and the sort
    /// direction is not explicitly defined ascending sorting is used.
    /// </summary>
    public SortDirection? SortDirection { get; set; }

    public override string ToString()
    {
        return Name != null
            ? $"<{nameof(DataSetListView)} {Name}>"
            : $"<{nameof(DataSetListView)}>";
    }
}