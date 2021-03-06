using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Kapok.Core;
using Kapok.Entity;

namespace Kapok.View;

/// <summary>
/// An error occurred during page construction.
/// </summary>
public class PageConstructionException : Exception
{
    public PageConstructionException(Type pageType, Exception innerException)
        // TODO translate exception message
        : base($"An error occurred during page construction. Page type: {pageType}")
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
    protected ViewDomain()
    {
        Culture = Thread.CurrentThread.CurrentUICulture;

        if (Default == null)
            Default = this;
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

    // TODO: maybe rename to 'RegisterEntityDefaultListPage' and create a new for RegisterEntityDefaultCardPage ? --> architecture decision required for that
    public static void RegisterEntityDefaultPage<TEntity>(Type pageType)
        where TEntity : class
    {
        if (!typeof(IDataPage).IsAssignableFrom(pageType))
            throw new ArgumentException("The default page type must inherit the interface IDataPage.", nameof(pageType));

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
        if (EntityDefaultPageOfEntityType.ContainsKey(entityType))
            return EntityDefaultPageOfEntityType[entityType];

        return null;
    }

    public abstract Type GetPageControlType(Type pageType);

    public IPage ConstructPage(Type pageType, Dictionary<Type, object?>? constructorParamValues)
    {
        var constructors = pageType.GetConstructors(BindingFlags.Public | BindingFlags.Static |
                                                    BindingFlags.Instance);
        ConstructorInfo? foundConstructorInfo = null;
        var constructorParameterValues = new List<object?>();

        foreach (var constructorInfo in constructors)
        {
            bool isValid = true;
            constructorParameterValues.Clear();
            foreach (var parameterInfo in constructorInfo.GetParameters())
            {
                if (constructorParamValues != null && constructorParamValues.ContainsKey(parameterInfo.ParameterType))
                {
                    constructorParameterValues.Add(constructorParamValues[parameterInfo.ParameterType]);
                }
                else if (parameterInfo.ParameterType == typeof(IViewDomain))
                {
                    constructorParameterValues.Add(this);
                }
                else if (parameterInfo.ParameterType == typeof(IDataDomainScope))
                {
                    if (DataDomain.Default == null)
                    {
                        isValid = false;
                        break;
                    }

                    var scope = DataDomain.Default.CreateScope();
                    constructorParameterValues.Add(scope);
                }
                else if (parameterInfo.HasDefaultValue)
                {
                    constructorParameterValues.Add(parameterInfo.DefaultValue);
                }
                else
                {
                    isValid = false;
                    break;
                }
            }

            if (!isValid)
                continue;

            foundConstructorInfo = constructorInfo;
            break;
        }

        if (foundConstructorInfo == null)
            throw new NotSupportedException("The TPage does not implement a constructor with a valid list of parameters in its constructor(s).");

        IPage newPage;
        try
        {
            newPage = (IPage)foundConstructorInfo.Invoke(constructorParameterValues.ToArray());
        }
        catch (BusinessLayerErrorException e)
        {
            throw new PageConstructionException(pageType, innerException: e);
        }
        catch (Exception e)
        {
            Debugger.Break();
            throw new PageConstructionException(pageType, innerException: e);
        }

        return newPage;
    }

    public IPage ConstructPage(Type pageType, IDataDomainScope? dataDomainScope = null)
    {
        return ConstructPage(pageType, new Dictionary<Type, object?>
        {
            {typeof(IDataDomainScope), dataDomainScope}
        });
    }

    public TPage ConstructPage<TPage>(IDataDomainScope? dataDomainScope = null)
        where TPage : IPage
    {
        var constructors = typeof(TPage).GetConstructors(BindingFlags.Public | BindingFlags.Static |
                                                         BindingFlags.Instance);
        ConstructorInfo? foundConstructorInfo = null;
        var constructorParameterValues = new List<object?>();

        foreach (var constructorInfo in constructors)
        {
            bool isValid = true;
            constructorParameterValues.Clear();
            foreach (var parameterInfo in constructorInfo.GetParameters())
            {
                if (parameterInfo.ParameterType == typeof(IDataDomainScope))
                {
                    constructorParameterValues.Add(dataDomainScope);
                }
                else
                {
                    isValid = false;
                    break;
                }
            }

            if (!isValid)
                continue;

            foundConstructorInfo = constructorInfo;
            break;
        }

        if (foundConstructorInfo == null)
            throw new NotSupportedException("The TPage does not implement a constructor with a valid list of ");

        TPage newPage;
        try
        {
            newPage = (TPage)foundConstructorInfo.Invoke(constructorParameterValues.ToArray());
        }
        catch (Exception e)
        {
            throw new PageConstructionException(typeof(TPage), innerException: e);
        }

        return newPage;
    }

    public abstract IQueryableView<TEntity> CreateQueryableView<TEntity>(IQueryable<TEntity> queryable)
        where TEntity : class;

    public abstract IPropertyLookupView CreatePropertyLookupView(ILookupDefinition lookupDefinition, IDataDomain dataDomain, IDataSetView? dataSet);

    public IDataPage ConstructEntityDefaultPage(Type entityType, IDataDomainScope? dataDomainScope = null)
    {
        var pageType = GetEntityDefaultPageType(entityType);
        if (pageType == null)
            throw new ArgumentException($"For the entity type {entityType.FullName} is no default page defined.",
                nameof(entityType));

        return (IDataPage)ConstructPage(pageType, dataDomainScope);
    }

    public abstract IDataSetView<TEntry> CreateDataSetView<TEntry>(IDataDomainScope dataDomainScope, IDao<TEntry>? repository = null)
        where TEntry : class, new();
    public abstract IHierarchyDataSetView<TEntry> CreateHierarchyDataSetView<TEntry>(IDataDomainScope dataDomainScope, IDao<TEntry>? repository = null)
        where TEntry : class, IHierarchyEntry<TEntry>, new();

    public abstract void RegisterPageContainer(IPage owningPage, ICollection<IPage> pageContainer);
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

    public abstract void BusinessLayerMessageEventToSingleUIMessage(object businessLayerObject, ReportBusinessLayerMessageEventArgs eventArgs);

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
    internal readonly Dictionary<(Type, string), List<(string, UIMenuItem)>> DynamicMenuItems = new();

    public void RegisterDynamicMenuItem(Type destinationType, UIMenuItem menuItem, string? menuName = null, string? menuPath = null)
    {
        var tuple = (destinationType, menuName ?? UIMenu.BaseMenuName);

        List<(string, UIMenuItem)> l1;
        if (DynamicMenuItems.ContainsKey(tuple))
            l1 = DynamicMenuItems[tuple];
        else
        {
            l1 = new List<(string, UIMenuItem)>();
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