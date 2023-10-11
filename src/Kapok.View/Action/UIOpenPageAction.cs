using System.Diagnostics;
using Kapok.BusinessLayer;

namespace Kapok.View;

// ReSharper disable once InconsistentNaming
public class UIOpenPageAction : UIAction, IOpenPageAction
{
    private static void DummyExecute()
    {
        throw new NotSupportedException("Internal implementation exception: The action of a UIOpenPageAction class or subclass has been called in a not correct way.");
    }

    private readonly Type? _pageType;
    private readonly IViewDomain? _viewDomain;
    private readonly IPage? _page;
    private DocumentPageCollectionPage? _hostPage;
    protected readonly Dictionary<Type, object>? PageConstructorParamValues;

    public UIOpenPageAction(string name, IPage page, Func<bool>? canExecute = null)
        : base(name, DummyExecute, canExecute)
    {
        ArgumentNullException.ThrowIfNull(page);

        // internal override
        ExecuteFunc = OpenPage;

        // main constructor code
        _page = page;
    }

    public UIOpenPageAction(string name, Type pageType, IViewDomain viewDomain, Func<bool>? canExecute = null)
        : base(name, DummyExecute, canExecute)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        ArgumentNullException.ThrowIfNull(viewDomain);

        // internal override
        ExecuteFunc = OpenPage;

        // main constructor code
        if (!typeof(IPage).IsAssignableFrom(pageType))
            throw new ArgumentException($"The pageType parameter must have a type which implements the interface {typeof(IPage).FullName}", nameof(pageType));

        _pageType = pageType;
        _viewDomain = viewDomain;
        PageConstructorParamValues = new Dictionary<Type, object>
        {
            { typeof(IViewDomain), _viewDomain }
        };
    }

    public IPage GetOrConstructPage()
    {
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8620
        return _page ?? _viewDomain.ConstructPage(_pageType, PageConstructorParamValues);
#pragma warning restore CS8620
#pragma warning restore CS8604
#pragma warning restore CS8602
    }

    public DocumentPageCollectionPage? HostPage
    {
        get => _hostPage;
        set => SetProperty(ref _hostPage, value);
    }

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
            _viewDomain?.ShowErrorMessage($"An error occurred during opening of page type {_pageType.FullName}", exception: e);
#pragma warning restore CS8602
            return;
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