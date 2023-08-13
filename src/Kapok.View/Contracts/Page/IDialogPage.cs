namespace Kapok.View;

public interface IDialogPage : IPage
{
    bool? DialogResult { get; }
    
    // actions
    IAction DefaultAction { get; }
    IAction CancelAction { get; }
}