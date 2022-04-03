using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace Kapok.View;

/// <summary>
/// A page which shows a collection of document pages (similar to Visual Studio).
/// </summary>
public partial class DocumentPageCollectionPage : InteractivePage
{
    private IPage? _selectedDocumentPage;
    private readonly Dictionary<IPage, List<UIMenuItemTab>> _contextualMenuItems = new();
    private Dictionary<IPage, object> _documentPageSource = new();

    protected DocumentPageCollectionPage(IViewDomain viewDomain) : base(viewDomain)
    {
        ViewDomain.RegisterPageContainer(this, DocumentPages);
        DocumentPages.CollectionChanged += Pages_CollectionChanged;

        // Actions
        CloseCurrentDocumentPageAction = new UIAction("CloseCurrentDocumentPage", CloseCurrentDocumentPage, CanCloseCurrentDocumentPage);
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
        return SelectedDocumentPage != null;
    }

    private void CloseCurrentDocumentPage()
    {
        if (SelectedDocumentPage != null)
            DocumentPages.Remove(SelectedDocumentPage);
    }

    #endregion

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
    public IPage? SelectedDocumentPage
    {
        get => _selectedDocumentPage;
        set
        {
            var oldPage = _selectedDocumentPage;
            if (SetProperty(ref _selectedDocumentPage, value))
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
        if (oldPage != null && oldPage is InteractivePage oldInteractivePage)
        {
            DetailPages.RemoveRange(oldInteractivePage.DetailPages);
        }

        if (page != null && page is InteractivePage interactivePage)
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
            SelectedDocumentPage = null;
        }

        if (_documentPageSource.ContainsKey(page))
            _documentPageSource.Remove(page);
    }
}