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
    /// The sorting of the list.
    /// </summary>
    public PropertyInfo[]? SortBy { get; set; }

    public override string ToString()
    {
        return Name != null
            ? $"<{nameof(DataSetListView)} {Name}>"
            : $"<{nameof(DataSetListView)}>";
    }
}