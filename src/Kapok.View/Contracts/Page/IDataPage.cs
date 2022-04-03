namespace Kapok.View;
// TODO: this is currently a bit dirty interface because it does not implement all available functions

public interface IDataPage : IInteractivePage
{
    IDataSetView DataSet { get; }

    IReadOnlyDictionary<string, IDataSetView> DataSets { get; }

    bool Editable { get; set; }
        
    DataPageEditMode EditMode { get; set; }
        
    bool IsEditable { get; }

    bool AllowCreateNewEntry { get; }

    bool AllowDeleteEntry { get; }

    /// <summary>
    /// Returns true when there is any DataSet in this page which
    /// has an validation error.
    /// </summary>
    bool HasValidationErrors { get; }
}
    
public interface IDataPage<TEntry> : IDataPage
    where TEntry : class, new()
{
    new IDataSetView<TEntry> DataSet { get; }
}