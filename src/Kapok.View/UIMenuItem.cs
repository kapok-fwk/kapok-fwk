using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Kapok.Entity;

namespace Kapok.View;

// ReSharper disable once InconsistentNaming
public class UIMenuItem : BindableObjectBase
{
    private string? _image;
    private bool _isVisible;
    protected bool IsVisibleIsSet;
    private bool _isVisibleDefault = true; // the default visible option
    private Caption _label = new();
    private Caption _description = new();
    private int _order;
    private string? _ribbonKeyTip; // TODO: this is specific to the current view logic and should maybe be replaced or moved into Kapok.View.Wpf
    private bool? _imageIsBig;

    public UIMenuItem(string name)
    {
        Name = name;

        SubMenuItems.CollectionChanged += SubMenuItems_CollectionChanged;
    }

    private void SubMenuItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach(var newItem in e.NewItems.Cast<UIMenuItem>())
            {
                newItem.PropertyChanged += SubMenuItem_PropertyChanged;
            }
        }
        if (e.OldItems != null)
        {
            foreach(var oldItem in e.OldItems.Cast<UIMenuItem>())
            {
                oldItem.PropertyChanged -= SubMenuItem_PropertyChanged;
            }
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                break;
            case NotifyCollectionChangedAction.Remove:
                break;
            case NotifyCollectionChangedAction.Replace:
                break;
        }
    }

    private void SubMenuItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender == null) return;

        var subMenuItem = (UIMenuItem)sender;

        if (e.PropertyName == nameof(IsVisible) &&
            !subMenuItem.IsVisible)
        {
            // check if all sub menu items are invisible
            var newIsVisibleDefault = SubMenuItems.Any(mi => mi.IsVisible);

            if (_isVisibleDefault != newIsVisibleDefault)
            {
                _isVisibleDefault = newIsVisibleDefault;
                OnPropertyChanged(nameof(IsVisible));
            }
        }
    }

    public string Name { get; }

    public string? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }

    public bool? ImageIsBig
    {
        get => _imageIsBig;
        set => SetProperty(ref _imageIsBig, value);
    }

    public virtual bool IsVisible
    {
        get => IsVisibleIsSet ? _isVisible : GetIsVisibleFromInherit();
        set
        {
            IsVisibleIsSet = true;
            SetProperty(ref _isVisible, value);
        }
    }

    protected virtual bool GetIsVisibleFromInherit()
    {
        return _isVisibleDefault;
    }

    public Caption Label
    {
        get => _label;
#pragma warning disable CS8601
        set => SetProperty(ref _label, value);
#pragma warning restore CS8601
    }

    public Caption Description
    {
        get => _description;
#pragma warning disable CS8601
        set => SetProperty(ref _description, value);
#pragma warning restore CS8601
    }

    public int Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }

    public string? RibbonKeyTip
    {
        get => _ribbonKeyTip;
        set => SetProperty(ref _ribbonKeyTip, value);
    }

    public ObservableCollection<UIMenuItem> SubMenuItems { get; set; } = new();
}