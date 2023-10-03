using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace Kapok.View;

/// <summary>
/// A page which shows a collection of document pages (similar to Visual Studio and old midi window forms).
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class DocumentPageCollectionPage : InteractivePage
{
    private IPage? _currentDocumentPage;
    private readonly Dictionary<IPage, List<UIMenuItemTab>> _contextualMenuItems = new();
    private readonly Dictionary<IPage, object> _documentPageSource = new();

    protected DocumentPageCollectionPage(IViewDomain? viewDomain = null) : base(viewDomain)
    {
        ViewDomain.UnregisterPageContainer(this);
        ViewDomain.RegisterPageContainer(this, DocumentPages);
        DocumentPages.CollectionChanged += Pages_CollectionChanged;

        // Actions
        CloseCurrentDocumentPageAction = new UIAction("CloseCurrentDocumentPage", CloseCurrentDocumentPage, CanCloseCurrentDocumentPage);
        CurrentDocumentSaveDataAction = new UIAction("SaveData", CurrentDocumentSaveData, CanCurrentDocumentSaveData);
        CurrentDocumentRefreshAction = new UIAction("Refresh", CurrentDocumentRefresh, CanCurrentDocumentRefresh);
        CurrentDocumentCreateNewEntryAction = new UIAction("CreateNewEntry", CurrentDocumentCreateNewEntry, CanCurrentDocumentCreateNewEntry);
        CurrentDocumentDeleteEntryAction = new UIAction("DeleteEntry", CurrentDocumentDeleteEntry, CanCurrentDocumentDeleteEntry);
        CurrentDocumentEditEntryAction = new UIAction("EditEntry", CurrentDocumentEditEntry, CanCurrentDocumentEditEntry);
        CurrentDocumentToggleFilterVisibleAction = new UIAction("ToggleFilterVisible", CurrentDocumentToggleFilterVisible, CanCurrentDocumentToggleFilterVisible);
        CurrentDocumentExportAsExcelSheetAction = new UIAction("ExportAsExcelSheet", CurrentDocumentExportAsExcelSheet, CanCurrentDocumentExportAsExcelSheet);
    }

    public override void Close()
    {
        DocumentPages.CollectionChanged -= Pages_CollectionChanged;
        base.Close();
    }

    #region Internal logic
        
    private void Pages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Replace)
            throw new NotSupportedException($"The replacement of items in the collection {nameof(DocumentPages)} is not supported");

        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (var page in e.NewItems.Cast<IPage>())
            {
                OnAddDocumentPage(page);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (var page in e.OldItems.Cast<IPage>())
            {
                OnCloseDocumentPage(page);
            }
        }
    }

    private void DetailPages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Replace)
            throw new NotSupportedException(
                "Calling 'replace' on the 'DetailPages' is not supported when the page is used in a page collection.");

        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            DetailPages.AddRange(e.NewItems.Cast<IDetailPage>());
        }

        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            DetailPages.RemoveRange(e.OldItems.Cast<IDetailPage>());
        }
    }

    protected override void OnClosing(CancelEventArgs eventArgs)
    {
        // NOTE: This functionality has not bee tested!
        foreach (var page in DocumentPages)
        {
            if (!page.CloseAction.CanExecute())
            {
                eventArgs.Cancel = true;
            }
        }

        if (eventArgs.Cancel == false)
            base.OnClosing(eventArgs);
    }

    protected override void OnClosed()
    {
        // close all pages if possible
        foreach (var page in DocumentPages.ToList())
        {
            Debug.Assert(page.CloseAction.CanExecute());
            page.CloseAction.Execute();
        }

        DocumentPages.CollectionChanged -= Pages_CollectionChanged;
        ViewDomain.UnregisterPageContainer(this);

        base.OnClosed();
    }

    private bool CanCloseCurrentDocumentPage()
    {
        return CurrentDocumentPage != null;
    }

    private void CloseCurrentDocumentPage()
    {
        if (CurrentDocumentPage != null)
            DocumentPages.Remove(CurrentDocumentPage);
    }

    #endregion

    /// <summary>
    /// Shows the page on this page.
    /// </summary>
    /// <param name="page">
    /// The page to be opened in this <see cref="DocumentPageCollectionPage"/>.
    /// </param>
    /// <param name="source">
    /// An optional reference to be passed to identify the page. This could be e.g. the menu item which opened this page.
    /// </param>
    public void ShowDocumentPage(IPage page, object? source = null)
    {
        DocumentPages.Add(page);

        if (source != null)
            _documentPageSource.Add(page, source);

        CurrentDocumentPage = page;
    }

    /// <summary>
    /// Returns the document page by the given source reference.
    /// </summary>
    /// <param name="source">
    /// The source reference passed over in <see cref="ShowDocumentPage"/>.
    /// </param>
    /// <returns></returns>
    public IPage? FindDocumentPageBySource(object source)
    {
        return _documentPageSource.FirstOrDefault(p => p.Value == source).Key;
    }

    /// <summary>
    /// Tells all open page actions in the given menu to open itself in this <see cref="DocumentPageCollectionPage"/> by setting the <c>HostPage</c> property
    /// to this page.
    /// </summary>
    public void PatchMenuToOpenHere(string menuName)
    {
        PatchMenuToOpenHere(Menu[menuName]);
    }

    /// <summary>
    /// Tells all open page actions in the given menu to open itself in this <see cref="DocumentPageCollectionPage"/> by setting the <c>HostPage</c> property
    /// to this page.
    /// </summary>
    public void PatchMenuToOpenHere(UIMenu menu)
    {
        foreach (var menuMenuItem in menu.MenuItems)
        {
            PatchMenuToOpenHere(menuMenuItem);
        }
    }

    /// <summary>
    /// Tells all open page actions in the given menu to open itself in this <see cref="DocumentPageCollectionPage"/> by setting the <c>HostPage</c> property
    /// to this page.
    /// </summary>
    public void PatchMenuToOpenHere(UIMenuItem menuItem)
    {
        if (menuItem is UIMenuItemAction menuItemAction)
        {
            if (menuItemAction.Action is IOpenPageAction openPageAction)
                openPageAction.HostPage = this;
        }
        else
        {
            var menuItemType = menuItem.GetType();
            if (menuItemType.IsGenericType && menuItemType.GetGenericTypeDefinition() == typeof(UIMenuItemAction<>))  // is generic
            {
                var hostPageProperty = menuItemType.GetProperty(nameof(IOpenPageAction.HostPage));
                hostPageProperty?.SetValue(menuItem, this);
            }
        }

        foreach (var menuItemSubMenuItem in menuItem.SubMenuItems)
        {
            PatchMenuToOpenHere(menuItemSubMenuItem);
        }
    }

    // ReSharper disable CollectionNeverQueried.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    /// <summary>
    /// The current selected document page.
    /// </summary>
    public IPage? CurrentDocumentPage
    {
        get => _currentDocumentPage;
        set
        {
            var oldPage = _currentDocumentPage;
            if (SetProperty(ref _currentDocumentPage, value))
            {
                OnSelectedDocumentPageChangedInternal(oldPage, value);
            }
        }
    }

    /// <summary>
    /// The document pages which are shown on the page.
    /// </summary>
    public ObservableCollection<IPage> DocumentPages { get; } = new();

    /// <summary>
    /// Closes the current selected document page (property SelectedDocumentPage).
    /// </summary>
    public IAction CloseCurrentDocumentPageAction { get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore UnusedMember.Global
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore CollectionNeverQueried.Global

    /// <summary>
    /// It is called internally when a document page is prepared for to be
    /// shown in the UI or the UI might already show the page.
    /// </summary>
    /// <param name="page"></param>
    private void ShowDocumentPageInternal(IPage page)
    {
        if (page is InteractivePage interactivePage)
        {
            // add menus
            if (interactivePage.Menu.ContainsKey(UIMenu.BaseMenuName))
            {
                var newMenuTabs = new List<UIMenuItemTab>();

                foreach (var menuItem in interactivePage.Menu[UIMenu.BaseMenuName].MenuItems)
                {
                    var newTabMenuItem =
                        new UIMenuItemTab(
                                $"{page.GetType().Name}_{menuItem.Name}") // NOTE: name might not be unique!
                            {
                                Label = menuItem.Label,
                                Description = menuItem.Description,
                                Image = menuItem.Image,
                                BasePage = menuItem is UIMenuItemTab menuItemTab ? menuItemTab.BasePage : page,
                                IsVisible = menuItem.IsVisible,  
                                Order = menuItem.Order,
                                RibbonKeyTip = menuItem.RibbonKeyTip,
                            };
                    newTabMenuItem.SubMenuItems.AddRange(menuItem.SubMenuItems);

                    newMenuTabs.Add(newTabMenuItem);
                }

                _contextualMenuItems.Add(page, newMenuTabs);
                Menu[UIMenu.BaseMenuName].MenuItems.AddRange(newMenuTabs);
            }
            
            // select first tab of contextual tab if exist
            if (_contextualMenuItems.ContainsKey(page))
            {
                if (_contextualMenuItems[page].Count > 0)
                {
                    _contextualMenuItems[page][0].IsSelected = true;
                }
            }
            
            // add detail pages
            DetailPages.AddRange(interactivePage.DetailPages);
            interactivePage.DetailPages.CollectionChanged += DetailPages_CollectionChanged;
        }
    }
    
    /// <summary>
    /// It is called internally when a document page is prepared for to be
    /// hidden in the UI or is already closed.
    /// </summary>
    /// <param name="page"></param>
    private void HideDocumentPageInternal(IPage page)
    {
        if (page is InteractivePage interactivePage)
        {
            // add menus
            if (_contextualMenuItems.ContainsKey(page))
            {
                Menu[UIMenu.BaseMenuName].MenuItems.RemoveRange(_contextualMenuItems[page]);
                _contextualMenuItems.Remove(page);
            }

            // if a Menu item exist which is not part of the page (e.g. a general App menu), show it
            if (Menu[UIMenu.BaseMenuName].MenuItems.Count > 0)
            {
                if (Menu[UIMenu.BaseMenuName].MenuItems[0] is UIMenuItemTab menuItemTab)
                {
                    menuItemTab.IsSelected = true;
                }
                else if (Menu[UIMenu.BaseMenuName].MenuItems[0].SubMenuItems.Count > 0)
                {
                    if (Menu[UIMenu.BaseMenuName].MenuItems[0].SubMenuItems[0] is UIMenuItemTab menuItemTab2)
                    {
                        menuItemTab2.IsSelected = true;
                    }
                }
            }
            
            // add detail pages
            interactivePage.DetailPages.CollectionChanged -= DetailPages_CollectionChanged;
            DetailPages.RemoveRange(interactivePage.DetailPages);
        }
        
        // select first tab of contextual tab if exist
        if (_contextualMenuItems.ContainsKey(page))
        {
            if (_contextualMenuItems[page].Count > 0)
            {
                _contextualMenuItems[page][0].IsSelected = true;
            }
        }
    }
    
    private void OnSelectedDocumentPageChangedInternal(IPage? oldPage, IPage? page)
    {
        if (oldPage != null)
            HideDocumentPageInternal(oldPage);
        if (page != null)
            ShowDocumentPageInternal(page);

        OnSelectedDocumentPageChanged(oldPage, page);
    }
    
    /// <summary>
    /// Is invoked when the selected page is changed.
    /// </summary>
    /// <param name="oldPage">
    /// The page which was selected before.
    /// </param>
    /// <param name="page">
    /// The page which is selected now.
    /// </param>
    protected virtual void OnSelectedDocumentPageChanged(IPage? oldPage, IPage? page)
    {
    }

    /// <summary>
    /// Is invoked when a new page is added to the Pages property.
    /// </summary>
    /// <param name="page">
    /// The new page object.
    /// </param>
    protected virtual void OnAddDocumentPage(IPage page)
    {
    }

    /// <summary>
    /// Is invoked when a page was removed from the Pages property.
    /// 
    /// This is invoked as well when the page is closed.
    /// </summary>
    /// <param name="page"></param>
    protected virtual void OnCloseDocumentPage(IPage page)
    {
        if (CurrentDocumentPage == page)
            HideDocumentPageInternal(page);

        // TODO: The WPF Avalon Dock is not stable here; as workaround we will add this code to make sure that when the last page is removed the 'SelectedDocumentPage' is set to null {to release as well the reference so that the GC can clean up the memory for the last document)
        if (DocumentPages.Count == 0)
        {
            CurrentDocumentPage = null;
        }

        if (_documentPageSource.ContainsKey(page))
            _documentPageSource.Remove(page);
    }

    #region Routed events to SelectedDocumentPage

    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public IAction CurrentDocumentSaveDataAction { get; }
    public IAction CurrentDocumentRefreshAction { get; }
    public IAction CurrentDocumentCreateNewEntryAction { get; }
    public IAction CurrentDocumentDeleteEntryAction { get; }
    public IAction CurrentDocumentEditEntryAction { get; }
    public IAction CurrentDocumentToggleFilterVisibleAction { get; }
    public IAction CurrentDocumentExportAsExcelSheetAction { get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    
    private bool CanCurrentDocumentSaveData()
    {
        if (CurrentDocumentPage is IDataPage dataPage)
        {
            return dataPage.SaveDataAction.CanExecute();
        }

        return false;
    }

    private void CurrentDocumentSaveData()
    {
        if (CurrentDocumentPage is IDataPage dataPage)
        {
            dataPage.SaveDataAction.Execute();
        }
    }

    private bool CanCurrentDocumentRefresh()
    {
        if (CurrentDocumentPage is IDataPage dataPage)
        {
            return dataPage.RefreshAction.CanExecute();
        }

        return false;
    }

    private void CurrentDocumentRefresh()
    {
        if (CurrentDocumentPage is IDataPage dataPage)
        {
            dataPage.RefreshAction.Execute();
        }
    }

    private bool CanCurrentDocumentCreateNewEntry()
    {
        if (CurrentDocumentPage is IDataPage dataPage)
        {
            return dataPage.CreateNewEntryAction.CanExecute();
        }

        return false;
    }

    private void CurrentDocumentCreateNewEntry()
    {
        if (CurrentDocumentPage is IDataPage dataPage)
        {
            dataPage.CreateNewEntryAction.Execute();
        }
    }

    private bool CanCurrentDocumentDeleteEntry()
    {
        if (CurrentDocumentPage is IDataPage dataPage)
        {
            return dataPage.DeleteEntryAction.CanExecute();
        }

        return false;
    }

    private void CurrentDocumentDeleteEntry()
    {
        if (CurrentDocumentPage is IDataPage dataPage)
        {
            dataPage.DeleteEntryAction.Execute();
        }
    }

    private bool CanCurrentDocumentEditEntry()
    {
        if (CurrentDocumentPage is IListPage listPage)
        {
            return listPage.EditEntryAction.CanExecute();
        }

        return false;
    }

    private void CurrentDocumentEditEntry()
    {
        if (CurrentDocumentPage is IListPage listPage)
        {
            listPage.EditEntryAction.Execute();
        }
    }

    private bool CanCurrentDocumentToggleFilterVisible()
    {
        if (CurrentDocumentPage is IListPage listPage)
        {
            return listPage.ToggleFilterVisibleAction.CanExecute();
        }

        return false;
    }

    private void CurrentDocumentToggleFilterVisible()
    {
        if (CurrentDocumentPage is IListPage listPage)
        {
            listPage.ToggleFilterVisibleAction.Execute();
        }
    }

    private bool CanCurrentDocumentExportAsExcelSheet()
    {
        if (CurrentDocumentPage is IListPage listPage)
        {
            return listPage.ExportAsExcelSheetAction.CanExecute();
        }

        return false;
    }

    private void CurrentDocumentExportAsExcelSheet()
    {
        if (CurrentDocumentPage is IListPage listPage)
        {
            listPage.ExportAsExcelSheetAction.Execute();
        }
    }

    #endregion
}