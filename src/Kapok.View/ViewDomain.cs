using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Entity;
using Kapok.Entity.Model;

namespace Kapok.View;

/// <summary>
/// An error occurred during page construction.
/// </summary>
public class PageConstructionException : Exception
{
    public PageConstructionException(Type pageType, Exception innerException)
        // TODO translate exception message
        : base($"An error occurred during page construction. Page type: {pageType}", innerException: innerException)
    {
    }
}

/// <summary>
/// A base class for the view domain.
/// 
/// A view domain serves as a instance per view e.g. a desktop. The class provides methods to interact
/// with the UI. In most of the use cases your application will only have one view.
/// </summary>
public abstract class ViewDomain : IViewDomain
{
    private IServiceProvider? _serviceProvider;

    protected ViewDomain(IServiceProvider? serviceProvider = default)
    {
        Culture = Thread.CurrentThread.CurrentUICulture;

        Default ??= this;

        if (serviceProvider != null)
            ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// The service provider to be used for page construction.
    /// </summary>
    public IServiceProvider ServiceProvider
    {
        get => _serviceProvider ??= CreateDefaultServiceProvider();
        set => _serviceProvider = value;
    }

    private IServiceProvider CreateDefaultServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IViewDomain>(p => this);
        return services.BuildServiceProvider();
    }

    public CultureInfo Culture { get; }

    public static ViewDomain? Default { get; set; }

    /// <summary>
    /// A action which must be defined my the main assembly which shuts down the application
    /// with a defined exit code.
    /// 
    /// If not initialized, the application will maybe not proper shut down, depending of
    /// its implementation.
    /// </summary>
    public Action<int>? ShutdownApplication { get; protected set; }

    private static readonly Dictionary<Type, Type> EntityDefaultPageOfEntityType = new();

    public static void RegisterEntityDefaultPage<TEntity>(Type pageType)
        where TEntity : class
    {
        if (!typeof(IPage).IsAssignableFrom(pageType))
            throw new ArgumentException(string.Format("The default page type must inherit the interface {0}.", typeof(IPage)), nameof(pageType));

        if (EntityDefaultPageOfEntityType.ContainsKey(typeof(TEntity)))
        {
            EntityDefaultPageOfEntityType[typeof(TEntity)] = pageType;
        }
        else
        {
            EntityDefaultPageOfEntityType.Add(typeof(TEntity), pageType);
        }
    }

    public Type? GetEntityDefaultPageType(Type entityType)
    {
        return EntityDefaultPageOfEntityType.TryGetValue(entityType, out var type) ? type : null;
    }

    public abstract Type GetPageControlType(Type pageType);

    protected IPage ConstructPage(Type pageType, IServiceProvider serviceProvider, params object[] parameters)
    {
        var page = (IPage?)serviceProvider.GetService(pageType);

        if (page != null)
            return page;

        // page is not registered in the provider, so we invoke the page creation ourselves
        try
        {
            page = (IPage)ActivatorUtilities.CreateInstance(serviceProvider, pageType, parameters);
        }
        catch (BusinessLayerErrorException e)
        {
            throw new PageConstructionException(pageType, innerException: e);
        }
        catch (Exception e)
        {
            throw new PageConstructionException(pageType, innerException: e);
        }

        return page;
    }

    public IPage ConstructPage(Type pageType)
    {
        return ConstructPage(pageType, ServiceProvider);
    }

    public IPage ConstructPage(Type pageType, IServiceProvider serviceProvider)
    {
        return ConstructPage(pageType, serviceProvider, Array.Empty<object>());
    }

    public IPage ConstructPage<TEntity>(Type pageType, IServiceProvider serviceProvider, IDataSetView<TEntity> dataSet)
        where TEntity : class, new()
    {
        var page = (IPage?)serviceProvider.GetService(pageType);

        var hasDataSetConstructor = pageType.GetConstructors().Any(c =>
            c.GetParameters().Any(p => p.ParameterType == typeof(IDataSetView<TEntity>)));

        if (page != null)
            return page;

        // page is not registered in the provider, so we invoke the page creation ourselves
        try
        {
            page = (IPage)ActivatorUtilities.CreateInstance(serviceProvider, pageType,
                hasDataSetConstructor ? new object[] { dataSet } : Array.Empty<object>()
            );
        }
        catch (BusinessLayerErrorException e)
        {
            throw new PageConstructionException(pageType, innerException: e);
        }
        catch (Exception e)
        {
            throw new PageConstructionException(pageType, innerException: e);
        }

        return page;
    }

    public TPage ConstructPage<TPage>()
        where TPage : IPage
    {
        return ConstructPage<TPage>(ServiceProvider);
    }

    public TPage ConstructPage<TPage>(IServiceProvider serviceProvider)
        where TPage : IPage
    {
        TPage newPage;
        try
        {
            newPage = ActivatorUtilities.CreateInstance<TPage>(serviceProvider);
        }
        catch (BusinessLayerErrorException e)
        {
            throw new PageConstructionException(typeof(TPage), innerException: e);
        }
        catch (Exception e)
        {
            throw new PageConstructionException(typeof(TPage), innerException: e);
        }

        return newPage;
    }

    public abstract IQueryableView<TEntity> CreateQueryableView<TEntity>(IQueryable<TEntity> queryable)
        where TEntity : class;

    public abstract IPropertyLookupView CreatePropertyLookupView(ILookupDefinition lookupDefinition, IDataDomain dataDomain, Func<object?>? currentSelector = null);

    IPropertyLookupView IViewDomain.CreatePropertyLookupView(ILookupDefinition lookupDefinition, IDataDomain dataDomain,
        IDataSetView? dataSet)
    {
        if (dataSet == null)
        {
            return CreatePropertyLookupView(lookupDefinition, dataDomain, null);
        }

        return CreatePropertyLookupView(lookupDefinition, dataDomain, () => dataSet.Current);
    }

    public IPage ConstructEntityDefaultPage(Type entityType, IServiceProvider serviceProvider)
    {
        var pageType = GetEntityDefaultPageType(entityType);
        if (pageType == null)
            throw new ArgumentException($"For the entity type {entityType.FullName} is no default page defined.",
                nameof(entityType));

        return ConstructPage(pageType, serviceProvider);
    }

    public abstract IDataSetView<TEntry> CreateDataSetView<TEntry>(IDataDomainScope dataDomainScope, IDao<TEntry>? repository = null)
        where TEntry : class, new();
    public abstract IHierarchyDataSetView<TEntry> CreateHierarchyDataSetView<TEntry>(IDataDomainScope dataDomainScope, IDao<TEntry>? repository = null)
        where TEntry : class, IHierarchyEntry<TEntry>, new();

    [Obsolete]
    public virtual void RegisterPageContainer(IPage owningPage, ICollection<IPage> pageContainer)
    {
        RegisterPageContainer(owningPage, (IEnumerable<IPage>)pageContainer);
    }
    public abstract void RegisterPageContainer(IPage owningPage, IEnumerable<IPage> pageContainer);
    public abstract void UnregisterPageContainer(IPage owningPage);

    public abstract void ShowInfoMessage(string message, string? title = null, IPage? ownerPage = null);
    public abstract void ShowErrorMessage(string message, string? title = null, IPage? ownerPage = null, Exception? exception = null);
    public abstract bool ShowYesNoQuestionMessage(string message, string? title = null, IPage? ownerPage = null);
    public abstract bool ShowConfirmMessage(string message, string? title = null, IPage? ownerPage = null);

    public abstract void ShowPage(IPage page);
    public abstract bool? ShowDialogPage(IPage page, IPage? ownerPage = null);
    public abstract void ClosePage(IPage page);
    public abstract void PageEndEdit(IPage page);

    public abstract void StartEditingDefaultDataGridCurrentEntity(IDataPage page, bool enforceFirstEditableRow);

    public abstract string? OpenOpenFileDialog(string title, string fileMask, IPage? ownerPage = null);
    public abstract string? OpenSaveFileDialog(string title, string fileMask, IPage? ownerPage = null);

    public abstract bool OpenReportDialog(object model, IDataDomain dataDomain, object? reportLayout = null, IPage? ownerPage = null);

    public abstract void OpenFile(string filename);

    public abstract void BusinessLayerMessageEventToSingleUIMessage(object? businessLayerObject, ReportBusinessLayerMessageEventArgs eventArgs);

    #region Menu

    /// <summary>
    /// Note:
    /// (
    ///  Type = destination page type
    ///  string = menu name
    /// )
    ///
    /// List: (
    ///     string = menu path
    ///     UIMenuItem = menu item
    /// )
    /// </summary>
    internal readonly Dictionary<(Type, string), List<(string?, UIMenuItem)>> DynamicMenuItems = new();

    public void RegisterDynamicMenuItem(Type destinationType, UIMenuItem menuItem, string? menuName = null, string? menuPath = null)
    {
        var tuple = (destinationType, menuName ?? UIMenu.BaseMenuName);

        if (!DynamicMenuItems.TryGetValue(tuple, out List<(string?, UIMenuItem)>? l1))
        {
            l1 = new List<(string?, UIMenuItem)>();
            DynamicMenuItems.Add(tuple, l1);
        }

        l1.Add((menuPath, menuItem));
    }

    public void RegisterDynamicMenuItem(string fullTypeName, UIMenuItem menuItem, string? menuName = null, string? menuPath = null)
    {
        // TODO: move this reflection code maybe somewhere else

        var destinationType =
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName?.Equals(fullTypeName) ?? false);

        //var destinationType = Type.GetType(fullTypeName);
        if (destinationType == null)
            throw new ArgumentException($"Could not find type '{fullTypeName}'.", nameof(fullTypeName));

        RegisterDynamicMenuItem(destinationType, menuItem, menuName ?? UIMenu.BaseMenuName, menuPath);
    }

    #endregion
}