using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using Res = Kapok.View.Resources.Menu.UIMenu;

namespace Kapok.View;

// ReSharper disable once InconsistentNaming
public class UIMenu
{
    public static string BaseMenuName = "Base";

    private readonly IPage _basePage;
    private ObservableCollection<UIMenuItem>? _menuItems;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="basePage">
    /// The page which hosts the menu.
    /// </param>
    public UIMenu(string name, IPage basePage)
    {
        Name = name;
        _basePage = basePage;
    }

    public string Name { get; }

    public IDataSetView? DefaultReferencingDataSet { get; set; }

    public ObservableCollection<UIMenuItem> MenuItems
    {
        get
        {
            if (_menuItems == null)
            {
                _menuItems = new ObservableCollection<UIMenuItem>();
                try
                {
                    BuildMenuFromType();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            return _menuItems;
        }
    }

    public string[] GroupSortOrder { get; set; } = Array.Empty<string>();

    private void AddMenuItem(UIMenuItem menuItem, IViewDomain viewDomain,
        string? tabName = null, int tabOrder = default,
        string? groupName = null, int groupOrder = default)
    {
        if (menuItem.Label.LanguageOrDefault(viewDomain.Culture) == default)
            menuItem.Label[string.Empty] = menuItem.Name;

        // set the ReferencingDataSet if not set for UIMenuItemDataSetSelectionAction<T> menu classes
        var menuItemType = menuItem.GetType();
        if (DefaultReferencingDataSet != null &&
            menuItemType.IsGenericType &&
            menuItemType.GetGenericTypeDefinition() == typeof(UIMenuItemDataSetSelectionAction<>))
        {
            var property =
                menuItemType.GetProperty(nameof(UIMenuItemDataSetSelectionAction<object>.ReferencingDataSet));
            Debug.Assert(property != null);
#pragma warning disable CS8602
            if (property.GetMethod.Invoke(menuItem, Array.Empty<object>()) == null)
            {
                property.SetMethod.Invoke(menuItem, new object[] {DefaultReferencingDataSet});
            }
#pragma warning restore CS8602
        }

        UIMenuItem? vmTab;
        if (string.IsNullOrEmpty(tabName))
        {
            var defaultMenuItem = MenuItems.FirstOrDefault(l => l.Label.LanguageOrDefault(viewDomain.Culture) == Res.DefaultMenuTab_Label);
            if (defaultMenuItem == null)
            {
                defaultMenuItem = CreateDefaultTabMenuItem();
                MenuItems.Add(defaultMenuItem);
            }

            vmTab = defaultMenuItem;
        }
        else
        {
            vmTab = MenuItems.FirstOrDefault(i => i.Label.LanguageOrDefault(viewDomain.Culture) == tabName);
            if (vmTab == null)
            {
                vmTab = new UIMenuItemTab(tabName);
                vmTab.Label[viewDomain.Culture] = tabName;
                vmTab.Order = tabOrder;
                MenuItems.Add(vmTab);
            }
        }

        string finalGroupName = groupName ?? Res.DefaultMenuGroup_Label;
        UIMenuItem? vmGroup = vmTab.SubMenuItems.FirstOrDefault(i => i.Label.LanguageOrDefault(viewDomain.Culture) == finalGroupName);
        if (vmGroup == null)
        {
            vmGroup = new UIMenuItem(finalGroupName);
            if (groupName == null)
            {
                vmGroup.Label[viewDomain.Culture] = finalGroupName;
                if (Array.FindIndex(GroupSortOrder, i => i == groupName) != -1)
                    vmGroup.Order = Array.FindIndex(GroupSortOrder, i => i == groupName);
                else
                    vmGroup.Order = 2;
            }
            else
            {
                vmGroup.Label[viewDomain.Culture] = groupName;
                vmGroup.Order = groupOrder;
            }
            vmTab.SubMenuItems.Add(vmGroup);
        }

        vmGroup.SubMenuItems.Add(menuItem);
    }

    private static UIMenuItem CreateDefaultTabMenuItem()
    {
        var defaultTabMenuItem = new UIMenuItem(Res.DefaultMenuTab_Label)
        {
            Label = Res.DefaultMenuTab_Label,
            Order = -1
        };
        return defaultTabMenuItem;
    }

    private void BuildMenuFromType()
    {
        string menuName = Name;
        Type type = _basePage.GetType();
        IViewDomain viewDomain = _basePage.ViewDomain;

        if (!typeof(IPage).IsAssignableFrom(type))
            throw new ArgumentException($"The type in parameter {nameof(type)} must inherit the interface {typeof(IPage).FullName}");

        // add the items
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.IsDefined(typeof(MenuItemAttribute)) &&
                        p.CanRead);
        foreach (var prop in props)
        {
            var menuItemAttribute = prop.GetCustomAttribute<MenuItemAttribute>();
            if (!(string.IsNullOrEmpty(menuItemAttribute?.MenuName) && menuName == BaseMenuName ||
                  menuItemAttribute?.MenuName == menuName))
            {
                continue;
            }

            if (prop.PropertyType == typeof(IToggleAction) ||
                prop.PropertyType == typeof(IAction))
            {
                // good
            }
            else if (prop.PropertyType.IsGenericType &&
                     prop.PropertyType.GetGenericTypeDefinition() == typeof(IDataSetSelectionAction<>))
            {
                // good
            }
            else
            {
                throw new NotSupportedException($"The property type {prop.PropertyType.FullName} is not supported in combination with the {nameof(MenuItemAttribute)} attribute.");
            }

            UIMenuItem menuItem;
#pragma warning disable CS8602
            object? action = prop.GetGetMethod(false).Invoke(_basePage, new object[] {});
#pragma warning restore CS8602
            if (action == null) // skip action which is not defined
                continue;

            if (action is IOpenPageAction openPageAction &&
                _basePage is DocumentPageCollectionPage pageCollectionPage)
            {
                action = new DocumentPageCollectionPage.OpenPageActionWrapper(openPageAction, pageCollectionPage);
            }

            if (prop.PropertyType == typeof(IToggleAction))
            {
                menuItem = new UIToggleMenuItemAction((IToggleAction)action);
            }
            else if (prop.PropertyType == typeof(IAction))
            {
                menuItem = new UIMenuItemAction((IAction)action);
            }
            else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() ==
                     typeof(IDataSetSelectionAction<>))
            {
                var entityType = prop.PropertyType.GenericTypeArguments[0];

                var newType = typeof(UIMenuItemDataSetSelectionAction<>).MakeGenericType(entityType);
                var constr = newType.GetConstructor(new[]
                {
                    typeof(IAction<>).MakeGenericType(typeof(IList<>).MakeGenericType(entityType)),
                    typeof(string)
                });
#pragma warning disable CS8602
                menuItem = (UIMenuItem)constr.Invoke(new []
#pragma warning restore CS8602
                {
                    action,
                    null
                });
            }
            else
            {
                continue;
            }

            string? groupName = null;
            int groupSortOrder = 2;

            var displayAttribute = prop.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null)
            {
                System.Resources.ResourceManager? resourceManager = (System.Resources.ResourceManager?)displayAttribute.ResourceType?
                    .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
                    .Invoke(null, null);

                if (!string.IsNullOrEmpty(displayAttribute.Name))
                {
                    var name = resourceManager?.GetString(displayAttribute.Name) ?? displayAttribute.Name;

                    menuItem.Label[viewDomain.Culture] = name;
                }

                if (!string.IsNullOrEmpty(displayAttribute.Description))
                {
                    menuItem.Description[viewDomain.Culture] = resourceManager?.GetString(displayAttribute.Description) ?? displayAttribute.Description;
                }

                if (!string.IsNullOrEmpty(displayAttribute.GroupName))
                {
                    groupName = resourceManager?.GetString(displayAttribute.GroupName) ?? displayAttribute.GroupName;
                    if (Array.FindIndex(GroupSortOrder, i => i == groupName) != -1)
                        groupSortOrder = Array.FindIndex(GroupSortOrder, i => i == groupName);
                }

                var displayAttributeOrder = displayAttribute.GetOrder();
                if (displayAttributeOrder != null)
                {
                    menuItem.Order = displayAttributeOrder.Value;
                }
            }

            AddMenuItem(menuItem, viewDomain,
#pragma warning disable CS8602
                menuItemAttribute.TabName, menuItemAttribute.Order,
#pragma warning restore CS8602
                groupName, groupSortOrder
            );
        }

        if (MenuItems.Count == 0)
        {
            var defaultTabMenuItem = new UIMenuItemTab(Res.DefaultMenuTab_Label)
            {
                // ReSharper disable once PossiblyMissingIndexerInitializerComma
                Label = new Caption()[viewDomain.Culture] = Res.DefaultMenuTab_Label,
                Order = -1
            };
            MenuItems.Add(defaultTabMenuItem);
        }


        // add static menu items registered through modules
        if (((ViewDomain)viewDomain).DynamicMenuItems.ContainsKey((type, menuName)))
        {
            foreach (var (menuPath, menuItem) in ((ViewDomain)viewDomain).DynamicMenuItems[(type, menuName)])
            {
                string? tabName = null;
                string? groupName = null;

                if (menuPath != null)
                {
                    var split = menuPath.Split('/');

                    tabName = split[0];

                    if (split.Length > 1)
                        groupName = split[1];
                }

                AddMenuItem(menuItem, viewDomain,
                    tabName: tabName,
                    groupName: groupName);
            }
        }


        // order the objections in the list. This could be probably be improved by using a list which already sorts entries on insert.
        var sortedMenuItems = MenuItems.OrderBy(m => m.Order).ToList();
        MenuItems.Clear();
        MenuItems.AddRange(sortedMenuItems);

        // TODO: move this into Kapok.View.Wpf
        string? FindRibbonKeyTip(UIMenuItem menuItem, IList<UIMenuItem> list)
        {
            if (menuItem.RibbonKeyTip != null)
                return menuItem.RibbonKeyTip;
            string? label = menuItem.Label.LanguageOrDefault(viewDomain.Culture);
            if (label == null)
                return null;
            if (label == string.Empty)
                return null;

            string keyTip = label[0].ToString();
            // ReSharper disable once AccessToModifiedClosure
            while (keyTip.Length < label.Length && list.Any(e => (e.Label.LanguageOrDefault(viewDomain.Culture)?.StartsWith(keyTip) ?? false) && e != menuItem))
            {
                keyTip += label[keyTip.Length].ToString(); // add one char to the KeyTip
            }

            return keyTip;
        }

        foreach (var vmTab in MenuItems)
        {
            if (vmTab.RibbonKeyTip == null)
            {
                vmTab.RibbonKeyTip = FindRibbonKeyTip(vmTab, MenuItems);
            }

            var sortedSubMenuItems1 = vmTab.SubMenuItems.OrderBy(m => m.Order).ToList();
            vmTab.SubMenuItems.Clear();
            vmTab.SubMenuItems.AddRange(sortedSubMenuItems1);

            foreach (var vmGroup in vmTab.SubMenuItems)
            {
                var sortedSubMenuItems2 = vmGroup.SubMenuItems.OrderBy(m => m.Order).ToList();
                vmGroup.SubMenuItems.Clear();
                vmGroup.SubMenuItems.AddRange(sortedSubMenuItems2);

                foreach (var actionMenuItem in vmGroup.SubMenuItems)
                {
                    if (actionMenuItem.RibbonKeyTip == null)
                    {
                        actionMenuItem.RibbonKeyTip = FindRibbonKeyTip(actionMenuItem, vmGroup.SubMenuItems);
                    }
                }
            }
        }
    }
}