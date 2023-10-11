using System.Collections.ObjectModel;
using Res = Kapok.View.Resources.InteractivePage;

namespace Kapok.View;

/// <summary>
/// A base class for a page with menu and detail pages.
/// </summary>
public abstract class InteractivePage : Page, IInteractivePage
{
    private readonly Dictionary<string, UIMenu> _menu;

    protected InteractivePage(IViewDomain? viewDomain = null)
        : base(viewDomain)
    {
        _menu = new Dictionary<string, UIMenu>();

        AddMenu(UIMenu.BaseMenuName);

        Menu[UIMenu.BaseMenuName].GroupSortOrder = new[] { Res.New, Res.Manage, Res.General, Res.Process, Res.Reports, "Group3", "Group4", "Group5", Res.SendTo, Res.Page };

        ViewDomain.RegisterPageContainer(this, DetailPages);
    }

    public override void Close()
    {
        ViewDomain.UnregisterPageContainer(this);
        base.Close();
    }

    protected void AddMenu(string menuName, IDataSetView? referencingDataSet = null)
    {
        if (string.IsNullOrEmpty(menuName))
            throw new ArgumentException($"Parameter {nameof(menuName)} can not be empty or null.", nameof(menuName));

        if (_menu.ContainsKey(menuName))
            throw new ArgumentException($"There exist already a menu with the name '{menuName}'.", nameof(menuName));

        _menu.Add(menuName, new UIMenu(menuName, this)
        {
            DefaultReferencingDataSet = referencingDataSet
        });
    }

    protected void AddMenu(UIMenu menu)
    {
        ArgumentNullException.ThrowIfNull(menu);

        if (string.IsNullOrEmpty(menu.Name))
            throw new ArgumentException($"{nameof(menu)}.Name can not be empty or null.", nameof(menu));
        
        if (_menu.ContainsKey(menu.Name))
            throw new ArgumentException($"There exist already a menu with the name '{menu.Name}'.", nameof(menu));

        _menu.Add(menu.Name, menu);
    }

    // ReSharper disable MemberCanBeProtected.Global
    public IReadOnlyDictionary<string, UIMenu> Menu => _menu;

    public ObservableCollection<IDetailPage> DetailPages { get; } = new();
    // ReSharper restore MemberCanBeProtected.Global

    #region IInteractivePage

    ICollection<IDetailPage> IInteractivePage.DetailPages => DetailPages;

    #endregion
}
