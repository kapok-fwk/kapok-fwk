namespace Kapok.View;

// ReSharper disable once InconsistentNaming
public class UIMenuItemDataSetSelectionAction<TEntry> : UIMenuItemAction<IList<TEntry?>?>, IDataSetSelectionAction<TEntry>
{
    private IDataSetView? _referencingDataSet;

    public UIMenuItemDataSetSelectionAction(IAction<IList<TEntry?>?> action, string? name = null) : base(action, name)
    {
    }

    /// <summary>
    /// The data set the action is referring to.
    /// </summary>
    public IDataSetView? ReferencingDataSet
    {
        get => _referencingDataSet;
        set => SetProperty(ref _referencingDataSet, value);
    }
}