using System.Collections.ObjectModel;
using Res = Kapok.View.Resources.InteractivePage;

namespace Kapok.View;

/// <summary>
/// A base class for a page with menu and detail pages.
/// </summary>
public abstract class InteractivePage : Page, IInteractivePage
{
    private readonly Dictionary<string, UIMenu> _menu;

    protected InteractivePage(IViewDomain? viewDomain)
        : base(viewDomain)
    {
        _menu = new Dictionary<string, UIMenu>();

        AddMenu(UIMenu.BaseMenuName);

        Menu[UIMenu.BaseMenuName].GroupSortOrder = new string[] { Res.New, Res.Manage, Res.General, Res.Process, Res.Reports, "Group3", "Group4", "Group5", Res.SendTo, Res.Page };
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

    // ReSharper disable MemberCanBeProtected.Global
    public IReadOnlyDictionary<string, UIMenu> Menu => _menu;

    public ObservableCollection<IDetailPage> DetailPages { get; } = new();
    // ReSharper restore MemberCanBeProtected.Global

    #region IInteractivePage

    ICollection<IDetailPage> IInteractivePage.DetailPages => this.DetailPages;

    #endregion
}