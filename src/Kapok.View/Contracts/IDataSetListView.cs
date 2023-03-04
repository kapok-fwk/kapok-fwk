namespace Kapok.View;

public interface IDataSetListView
{
    string? Name { get; set; }

    /// <summary>
    /// Name shown to the user for the list view.
    /// </summary>
    Caption? DisplayName { get; set; }

    /// <summary>
    /// INTERNAL. Used in WPF as command to select the DataSetListView as current ListView of e.g. a ListPage&lt;T&gt;.
    /// </summary>
    // not nice; necessary for toolbar menu select in WPF
    IAction? SelectAction { get; set; }

    /// <summary>
    /// The columns which shall be shown in the UI list.
    /// </summary>
    List<ColumnPropertyView>? Columns { get; set; }
}