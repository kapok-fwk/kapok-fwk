namespace Kapok.View;

public class UIMenuItemTab : UIMenuItem
{
    public UIMenuItemTab(string name) : base(name)
    {
    }

    private IPage? _basePage;
    private bool _isSelected;

    /// <summary>
    /// The base page this tab is referring to.
    /// When not set the current page is estimated.
    ///
    /// NOTE: This is currently only used in PageCollectionPage to separate the different
    /// document pages in the Ribbon control (see RibbonTab.ContextualTabGroupHeader).
    /// </summary>
    public IPage? BasePage
    {
        get => _basePage;
        set => SetProperty(ref _basePage, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}