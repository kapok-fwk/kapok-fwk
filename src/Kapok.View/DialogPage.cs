namespace Kapok.View;

/// <summary>
/// A base class for a dialog page.
/// </summary>
public abstract class DialogPage : Page, IDialogPage
{
    protected DialogPage(IViewDomain? viewDomain)
        : base(viewDomain)
    {
        DefaultAction = new UIAction("DialogDefaultAction", Action, CanAction);
        CancelAction = new UIAction("CancelDialog", Cancel);
    }

    public bool? DialogResult { get; protected set; }

    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public IAction DefaultAction { get; }
    public IAction CancelAction { get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore MemberCanBePrivate.Global

    protected virtual bool CanAction()
    {
        return true;
    }

    protected virtual void Action()
    {
        DialogResult = true;
        Close();
    }

    protected virtual void Cancel()
    {
        DialogResult = false;
        Close();
    }
}