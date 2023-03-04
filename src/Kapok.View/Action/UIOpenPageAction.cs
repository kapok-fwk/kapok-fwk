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
    protected readonly Dictionary<Type, object>? PageConstructorParamValues;

    public UIOpenPageAction(string name, IPage page, Func<bool>? canExecute = null)
        : base(name, DummyExecute, canExecute)
    {
        // internal override
        ExecuteFunc = OpenPage;

        // main constructor code
        _page = page ?? throw new ArgumentNullException(nameof(page));
    }
        
    public UIOpenPageAction(string name, Type pageType, IViewDomain viewDomain, Func<bool>? canExecute = null)
        : base(name, DummyExecute, canExecute)
    {
        // internal override
        ExecuteFunc = OpenPage;

        // main constructor code
        if (!typeof(IPage).IsAssignableFrom(pageType))
            throw new ArgumentException($"The pageType parameter must have a type which implements the interface {typeof(IPage).FullName}", nameof(pageType));

        _pageType = pageType ?? throw new ArgumentNullException(nameof(pageType));
        _viewDomain = viewDomain ?? throw new ArgumentNullException(nameof(viewDomain));
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

    private void OpenPage()
    {
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

        page.Show();
    }
}