using System.ComponentModel;

namespace Kapok.View;

// ReSharper disable once InconsistentNaming
public class UIToggleMenuItemAction : UIMenuItemAction, IToggleAction
{
    public UIToggleMenuItemAction(IToggleAction action, string? name = null) : base(action, name)
    {
    }

    protected override void PassActionOnPropertyChangedEvent(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IToggleAction.IsChecked))
        {
            OnPropertyChanged(nameof(IsChecked));
        }

        base.PassActionOnPropertyChangedEvent(sender, e);
    }

    public bool IsChecked
    {
        get => ((IToggleAction) Action).IsChecked;
        set => ((IToggleAction) Action).IsChecked = value;
    }
}