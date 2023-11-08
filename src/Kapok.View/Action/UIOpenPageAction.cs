using System.Diagnostics;
using Kapok.BusinessLayer;
using Microsoft.Extensions.DependencyInjection;

namespace Kapok.View;

// ReSharper disable once InconsistentNaming
public class UIOpenPageAction : UIAction, IOpenPageAction
{
    private static void DummyExecute()
    {
        throw new NotSupportedException("Internal implementation exception: The action of a UIOpenPageAction class or subclass has been called in a not correct way.");
    }

    private readonly Type? _pageType;
    private readonly IServiceProvider? _serviceProvider;
    private readonly IPage? _page;
    private DocumentPageCollectionPage? _hostPage;

    public UIOpenPageAction(string name, IPage page, Func<bool>? canExecute = null)
        : base(name, DummyExecute, canExecute)
    {
        ArgumentNullException.ThrowIfNull(page);

        // internal override
        ExecuteFunc = OpenPage;

        // main constructor code
        _page = page;
    }

    public UIOpenPageAction(string name, Type pageType, IServiceProvider serviceProvider, Func<bool>? canExecute = null)
        : base(name, DummyExecute, canExecute)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        // internal override
        ExecuteFunc = OpenPage;

        // main constructor code
        if (!typeof(IPage).IsAssignableFrom(pageType))
            throw new ArgumentException($"The pageType parameter must have a type which implements the interface {typeof(IPage).FullName}", nameof(pageType));

        _pageType = pageType;
        _serviceProvider = serviceProvider;
    }

    public IPage GetOrConstructPage()
    {
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8620
        return _page ?? _serviceProvider.GetRequiredService<IViewDomain>().ConstructPage(_pageType);
#pragma warning restore CS8620
#pragma warning restore CS8604
#pragma warning restore CS8602
    }

    public DocumentPageCollectionPage? HostPage
    {
        get => _hostPage;
        set => SetProperty(ref _hostPage, value);
    }

    /// <summary>
    /// The list view of the page which shall be selected before the page is
    /// opened.
    /// </summary>
    public string? ListViewName { get; init; }

    private void OpenPage()
    {
        if (HostPage != null)
        {
            // Check if the page is already opened. If yes: Jump to it
            var existingDocumentPage = HostPage.FindDocumentPageBySource(this);
            if (existingDocumentPage != null)
            {
                HostPage.CurrentDocumentPage = existingDocumentPage;
                return;
            }
        }

        IPage page;

        try
        {
            page = GetOrConstructPage();
        }
        catch (BusinessLayerErrorException)
        {
            return;
        }
        catch (Exception e)
        {
            Debugger.Break();
#pragma warning disable CS8602
            // TODO: translation is missing
            _serviceProvider?.GetService<IViewDomain>()?
                .ShowErrorMessage($"An error occurred during opening of page type {_pageType.FullName}", exception: e);
#pragma warning restore CS8602
            return;
        }

        if (ListViewName != null)
        {
            if (page is not IListPage listPage)
            {
                Debug.Fail($"UIOpenReferencedPageAction<> has been called with ListViewName '{ListViewName}' set but the page is no list view type. The parameter will be ignored.");
            }
            else
            {
                var listView = listPage.ListViews.FirstOrDefault(lv => lv.Name == ListViewName);
                listPage.CurrentListView = listView;
            }
        }

        if (HostPage != null)
        {
            HostPage.ShowDocumentPage(page, this);
        }
        else
        {
            page.Show();
        }
    }
}