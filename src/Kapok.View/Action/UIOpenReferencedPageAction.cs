using System.Diagnostics;
using Kapok.BusinessLayer;

namespace Kapok.View;

// ReSharper disable once InconsistentNaming
public class UIOpenReferencedPageAction<TEntry> : UIDataSetSingleSelectionAction<TEntry>, IOpenPageAction
    where TEntry : class, new()
{
    private static void DummyExecute(TEntry selected)
    {
        throw new Exception();
    }

    private readonly Type _pageType;
    private readonly IViewDomain? _viewDomain;
    private readonly IDataPage? _page;
    private DocumentPageCollectionPage? _hostPage;
    protected readonly Dictionary<Type, object>? PageConstructorParamValues;
    private readonly IDataSetView<TEntry>? _baseDataSetView;
    private readonly Action<IFilterSet, TEntry, IReadOnlyDictionary<string, object?>>? _filter;

    private readonly string? _listViewName;

    public UIOpenReferencedPageAction(string name, IDataPage page,
        IDataSetView<TEntry>? baseDataSetView = null,
        Action<IFilterSet, TEntry, IReadOnlyDictionary<string, object?>>? filter = null,
        Func<TEntry, bool>? canExecute = null, string? listViewName = null)
        : base(name, DummyExecute, canExecute)
    {
        // internal override
        ExecuteFunc = OpenPage;

        // main constructor code
        _page = page ?? throw new ArgumentNullException(nameof(page));
        _pageType = page.GetType();
        _baseDataSetView = baseDataSetView;
        _filter = filter;
        _listViewName = listViewName;
    }

    public UIOpenReferencedPageAction(string name, Type pageType, IViewDomain viewDomain,
        IDataSetView<TEntry>? baseDataSetView = null,
        Action<IFilterSet, TEntry, IReadOnlyDictionary<string, object?>>? filter = null,
        Func<TEntry, bool>? canExecute = null, string? listViewName = null)
        : base(name, DummyExecute, canExecute)
    {
        // internal override
        ExecuteFunc = OpenPage;

        // main constructor code
        if (!typeof(IDataPage).IsAssignableFrom(pageType))
            throw new ArgumentException($"The pageType parameter must have a type which implements the interface {typeof(IDataPage<>).FullName}", nameof(pageType));

        _pageType = pageType ?? throw new ArgumentNullException(nameof(pageType));
        _viewDomain = viewDomain ?? throw new ArgumentNullException(nameof(viewDomain));
        PageConstructorParamValues = new Dictionary<Type, object>
        {
            { typeof(IViewDomain), _viewDomain }
        };
        if (baseDataSetView != null)
            PageConstructorParamValues.Add(typeof(IDataSetView<TEntry>), baseDataSetView);
        _baseDataSetView = baseDataSetView;
        _filter = filter;
        _listViewName = listViewName;
    }

    private Type GetTSourceEntryType()
    {
        foreach (var @interface in _pageType.GetInterfaces())
        {
            if (@interface.IsGenericType &&
                @interface.GetGenericTypeDefinition() == typeof(IDataPage<>))
            {
                return @interface.GenericTypeArguments[0];
            }
        }

        throw new NotImplementedException("The TPage type does not implement IDataPage<TEntry>.");
    }

    private IFilterSet ConstructFilterSet()
    {
        var filterSetType = typeof(FilterSet<>).MakeGenericType(GetTSourceEntryType());

        var emptyConstructorInfo = filterSetType.GetConstructor(new Type[] { });
        if (emptyConstructorInfo == null)
            throw new NotSupportedException("Internal error: FilterSet<> does not have an empty constructor.");

        return (IFilterSet)emptyConstructorInfo.Invoke(null);
    }

    public IDataPage GetOrConstructPage()
    {
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8620
        return (IDataPage)_viewDomain.ConstructPage(_pageType, PageConstructorParamValues);
#pragma warning restore CS8620
#pragma warning restore CS8604
#pragma warning restore CS8602
    }

    public DocumentPageCollectionPage? HostPage
    {
        get => _hostPage;
        set => SetProperty(ref _hostPage, value);
    }

    private void OpenPage(TEntry selectedEntry)
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

        var page = _page;
        if (page == null)
        {
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
                // TODO: translation is missing
#pragma warning disable CS8602
                _viewDomain.ShowErrorMessage($"An error occurred during opening of page type {_pageType.FullName}", exception: e);
#pragma warning restore CS8602
                return;
            }
        }

        if (_filter != null)
        {
            var filterSet = ConstructFilterSet();

            var firstEntry = selectedEntry;
            var nestedDataFilter = _baseDataSetView?.Filter.GetNestedDataFilter(page.ViewDomain);
            _filter.Invoke(filterSet, firstEntry, nestedDataFilter ?? new Dictionary<string, object?>());

            page.DataSet.Filter.Add(filterSet);
        }

        if (_listViewName != null)
        {
            if (page is not IListPage listPage)
            {
                Debug.Fail($"UIOpenReferencedPageAction<> has been called with ListViewName '{_listViewName}' set but the page is no list view type. The parameter will be ignored.");
            }
            else
            {
                var listView = listPage.ListViews.FirstOrDefault(lv => lv.Name == _listViewName);
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

    #region IOpenPageAction

    IPage IOpenPageAction.GetOrConstructPage()
    {
        return GetOrConstructPage();
    }

    #endregion
}