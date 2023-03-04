using System.Collections;

namespace Kapok.View;

/// <summary>
/// An action getting a list of selected entries from a DataSet.
/// </summary>
// ReSharper disable once InconsistentNaming
public class UIDataSetSelectionAction<TEntry> : UIAction<IList<TEntry?>?>, IDataSetSelectionAction<TEntry>, IDataSetSelectionAction
    where TEntry : class
{
    public UIDataSetSelectionAction(string name, Action<IList<TEntry?>?> execute, Func<IList<TEntry?>?, bool>? canExecute = null)
#pragma warning disable CS8620
        : base(name, execute, canExecute)
#pragma warning restore CS8620
    {
    }

    public override void Execute(IList<TEntry?>? arg)
    {
        if (arg == null || arg.Count == 0)
            return;

        base.Execute(arg);
    }

    public override bool CanExecute(IList<TEntry?>? arg)
    {
        if (arg == null || arg.Count == 0)
            return false;

        return base.CanExecute(arg);
    }

    #region IDataSetSelectionAction / IAction<IList>

    bool IAction<IList?>.CanExecute(IList? arg)
    {
        return CanExecute(arg?.Cast<TEntry?>().ToList());
    }

    void IAction<IList?>.Execute(IList? arg)
    {
        Execute(arg?.Cast<TEntry?>().ToList());
    }

    #endregion
}