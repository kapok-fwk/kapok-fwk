using System.Collections;

namespace Kapok.View;

/// <summary>
/// An action getting the select entry from a DataSet.
/// When more than one entry is selected, method CanExecute will return false.
/// </summary>
// ReSharper disable once InconsistentNaming
public class UIDataSetSingleSelectionAction<TEntry> : UIAction<IList<TEntry?>?>, IDataSetSelectionAction<TEntry>, IDataSetSelectionAction
    where TEntry : class
{
    private static void DummyExecute(IList<TEntry?>? arg)
    {
        throw new NotSupportedException("Internal implementation exception: The action of a UIDataSetSingleSelectionAction<TEntry> class or subclass has been called in a not correct way.");
    }
    private static bool DummyCanExecute(IList<TEntry?>? arg)
    {
        throw new NotSupportedException("Internal implementation exception: The action of a UIDataSetSingleSelectionAction<TEntry> class or subclass has been called in a not correct way.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">
    /// The name of the action
    /// </param>
    /// <param name="execute">
    /// The action with the selected entry.
    ///
    /// The parameter of the function has the selected entry. This will never be null.
    /// </param>
    /// <param name="canExecute">
    /// The function to test if the action can be called
    /// for the selected entry.
    ///
    /// The parameter of the function has the selected entry. This will never be null.
    /// </param>
    public UIDataSetSingleSelectionAction(string name, Action<TEntry> execute, Func<TEntry, bool>? canExecute = null)
        : base(name, DummyExecute, DummyCanExecute)
    {
#pragma warning disable CS8622
        base.ExecuteFunc = InternalOverrideExecute;
        base.CanExecuteFunc = InternalOverrideCanExecute;
#pragma warning restore CS8622
        this.ExecuteFunc = execute ?? throw new ArgumentNullException(nameof(execute));
        this.CanExecuteFunc = canExecute;
    }

    internal new Action<TEntry> ExecuteFunc;
    internal new Func<TEntry, bool>? CanExecuteFunc;

    private void InternalOverrideExecute(IList<TEntry?> arg)
    {
        var entry = arg[0];
        if (entry == null)
            return;

        ExecuteFunc.Invoke(entry);
    }

    private bool InternalOverrideCanExecute(IList<TEntry?> arg)
    {
        var entry = arg[0];
        if (entry == null)
            return false;

        return CanExecuteFunc?.Invoke(entry) ?? true;
    }

    public override void Execute(IList<TEntry?>? arg)
    {
        if (arg == null || arg.Count != 1)
            return;

        base.Execute(arg);
    }

    public override bool CanExecute(IList<TEntry?>? arg)
    {
        if (arg == null || arg.Count != 1)
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