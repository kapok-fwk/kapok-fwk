namespace Kapok.View;

// ReSharper disable once InconsistentNaming
public class UIToggleAction : UIAction, IToggleAction
{
    private bool _isChecked;

    public UIToggleAction(string name, Action execute, Func<bool>? canExecute = null)
        : base(name, execute, canExecute)
    {
    }

    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}