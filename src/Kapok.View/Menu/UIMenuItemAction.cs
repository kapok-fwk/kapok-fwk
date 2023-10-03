using System.ComponentModel;

namespace Kapok.View;

// ReSharper disable once InconsistentNaming
public class UIMenuItemAction : UIMenuItem, IAction
{
    public UIMenuItemAction(IAction action, string? name = null)
        : base(name ?? action.Name)
    {
        Action = action;
        Image = action.Image;
        ImageIsBig = action.ImageIsBig;

        if (action is INotifyPropertyChanged actionNotifyPropertyChanged)
        {
            actionNotifyPropertyChanged.PropertyChanged += PassActionOnPropertyChangedEvent;
        }
    }

    public IAction Action { get; }

    protected virtual void PassActionOnPropertyChangedEvent(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IAction.IsVisible) && !IsVisibleIsSet)
        {
            OnPropertyChanged(nameof(IsVisible));
        }
    }

    protected override bool GetIsVisibleFromInherit()
    {
        return Action.IsVisible;
    }

    #region IAction members

    public event EventHandler? CanExecuteChanged
    {
        add => Action.CanExecuteChanged += value;
        remove => Action.CanExecuteChanged -= value;
    }

    public bool CanExecute()
    {
        return Action.CanExecute();
    }

    public void Execute()
    {
        Action.Execute();
    }

    #endregion
}

// ReSharper disable once InconsistentNaming
public class UIMenuItemAction<T> : UIMenuItem, IAction<T>
{
    public UIMenuItemAction(IAction<T> action, string? name = null)
        : base(name ?? action.Name)
    {
        Action = action;
        Image = action.Image;
        ImageIsBig = action.ImageIsBig;

        if (action is INotifyPropertyChanged actionNotifyPropertyChanged)
        {
            actionNotifyPropertyChanged.PropertyChanged += PassActionOnPropertyChangedEvent;
        }
    }

    public IAction<T> Action { get; }

    protected virtual void PassActionOnPropertyChangedEvent(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IAction.IsVisible) && !IsVisibleIsSet)
        {
            OnPropertyChanged(nameof(IsVisible));
        }
    }

    protected override bool GetIsVisibleFromInherit()
    {
        return Action.IsVisible;
    }

    #region IAction members

    public event EventHandler? CanExecuteChanged
    {
        add => Action.CanExecuteChanged += value;
        remove => Action.CanExecuteChanged -= value;
    }

    public bool CanExecute(T? arg)
    {
        return Action.CanExecute(arg);
    }

    public void Execute(T? arg)
    {
        Action.Execute(arg);
    }

    #endregion
}