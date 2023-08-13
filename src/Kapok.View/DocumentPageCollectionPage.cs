using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace Kapok.View;

/// <summary>
/// A page which shows a collection of document pages (similar to Visual Studio).
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public partial class DocumentPageCollectionPage : InteractivePage
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

    // ReSharper disable once ReturnTypeCanBeNotNullable
    private IPage? FindDocumentPageBySource(object source)
    {
        return _documentPageSource.FirstOrDefault(p => p.Value == source).Key;
    }

    private void AddDocumentPageWithSource(IPage page, object? source)
    {
        DocumentPages.Add(page);
        if (source != null)
            _documentPageSource.Add(page, source);
    }

    // ReSharper disable CollectionNeverQueried.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedMember.Global
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
                OnSelectedDocumentPageChanged(oldPage, value);
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
    // ReSharper restore UnusedMember.Global
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore CollectionNeverQueried.Global

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
        // switch detail pages
        if (oldPage is InteractivePage oldInteractivePage)
        {
            DetailPages.RemoveRange(oldInteractivePage.DetailPages);
        }

        if (page is InteractivePage interactivePage)
        {
            DetailPages.AddRange(interactivePage.DetailPages);
        }

        // select first tab of contextual tab if exist
        if (page != null && _contextualMenuItems.ContainsKey(page))
        {
            if (_contextualMenuItems[page].Count > 0)
            {
                _contextualMenuItems[page][0].IsSelected = true;
            }
        }
    }

    /// <summary>
    /// Is invoked when a new page is added to the Pages property.
    /// </summary>
    /// <param name="page">
    /// The new page object.
    /// </param>
    protected virtual void OnAddDocumentPage(IPage page)
    {
        if (page is InteractivePage interactivePage)
        {
            if (interactivePage.Menu.ContainsKey(UIMenu.BaseMenuName))
            {
                List<UIMenuItemTab> newMenuTabs = new List<UIMenuItemTab>();

                foreach (var tabMenuItem in interactivePage.Menu[UIMenu.BaseMenuName].MenuItems)
                {
                    var newTabMenuItem =
                        new UIMenuItemTab(
                                $"{page.GetType().Name}_{tabMenuItem.Name}") // TODO: name might not be unique!
                            {
                                Label = tabMenuItem.Label,
                                Description = tabMenuItem.Description,
                                Image = tabMenuItem.Image,
                                BasePage = page
                                // TODO: IsVisible is not transfered
                                // TODO: Order is not taken over
                                // TODO: RibbonKeyTip is not rewritten here
                            };
                    newTabMenuItem.SubMenuItems.AddRange(tabMenuItem.SubMenuItems);

                    newMenuTabs.Add(newTabMenuItem);
                }

                _contextualMenuItems.Add(page, newMenuTabs);
                Menu[UIMenu.BaseMenuName].MenuItems.AddRange(newMenuTabs);
            }

            interactivePage.DetailPages.CollectionChanged += DetailPages_CollectionChanged;
        }
    }

    /// <summary>
    /// Is invoked when a page was removed from the Pages property.
    /// 
    /// This is invoked as well when the page is closed.
    /// </summary>
    /// <param name="page"></param>
    protected virtual void OnCloseDocumentPage(IPage page)
    {
        if (page is InteractivePage interactivePage)
        {
            interactivePage.DetailPages.CollectionChanged -= DetailPages_CollectionChanged;
            DetailPages.RemoveRange(interactivePage.DetailPages);

            if (_contextualMenuItems.ContainsKey(page))
            {
                Menu[UIMenu.BaseMenuName].MenuItems.RemoveRange(_contextualMenuItems[page]);
                _contextualMenuItems.Remove(page);
            }
        }

        // TODO: The WPF Avalon Dock is not stable here; as workaround we will add this code to make sure that when the last page is removed the 'SelectedDocumentPage' is set to null {to release as well the reference so that the GC can clean up the memory for the last document)
        if (DocumentPages.Count == 0)
        {
            CurrentDocumentPage = null;
        }

        if (_documentPageSource.ContainsKey(page))
            _documentPageSource.Remove(page);
    }

    #region Routed events to SelectedDocumentPage

    public IAction CurrentDocumentSaveDataAction { get; }
    public IAction CurrentDocumentRefreshAction { get; }
    public IAction CurrentDocumentCreateNewEntryAction { get; }
    public IAction CurrentDocumentDeleteEntryAction { get; }
    public IAction CurrentDocumentEditEntryAction { get; }
    public IAction CurrentDocumentToggleFilterVisibleAction { get; }
    public IAction CurrentDocumentExportAsExcelSheetAction { get; }
    
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