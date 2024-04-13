using System.Globalization;
using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Entity;
using Kapok.Entity.Model;

namespace Kapok.View;

public interface IViewDomain
{
    CultureInfo Culture { get; }

    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// A action which must be defined my the main assembly which shuts down the application
    /// with a defined exit code.
    /// 
    /// If not initialized, the application will maybe not proper shut down, depending of
    /// its implementation.
    /// </summary>
    Action<int>? ShutdownApplication { get; }

    Type? GetEntityDefaultPageType(Type entityType);

    Type GetPageControlType(Type pageType);

    IPage ConstructPage(Type pageType);
    IPage ConstructPage(Type pageType, IServiceProvider serviceProvider);
    IPage ConstructPage<TEntity>(Type pageType, IServiceProvider serviceProvider, IDataSetView<TEntity> dataSet)
        where TEntity : class, new();

    TPage ConstructPage<TPage>()
        where TPage : IPage;

    TPage ConstructPage<TPage>(IServiceProvider serviceProvider)
        where TPage : IPage;

    IPage ConstructEntityDefaultPage(Type entityType, IServiceProvider serviceProvider);

    IQueryableView<TEntity> CreateQueryableView<TEntity>(IQueryable<TEntity> queryable)
        where TEntity : class;

    IPropertyLookupView? CreatePropertyLookupView(ILookupDefinition lookupDefinition, IDataDomain dataDomain, Func<object?>? currentSelector = null);
    [Obsolete("Please use CreatePropertyLookupView(ILookupDefinition, IDataDomain, Func<object?>?) instead")]
    IPropertyLookupView? CreatePropertyLookupView(ILookupDefinition lookupDefinition, IDataDomain dataDomain, IDataSetView? dataSet = null);
    IDataSetView<TEntry> CreateDataSetView<TEntry>(IDataDomainScope dataDomainScope, IEntityService<TEntry>? entityService = null)
        where TEntry : class, new();
    IHierarchyDataSetView<TEntry> CreateHierarchyDataSetView<TEntry>(IDataDomainScope dataDomainScope, IEntityService<TEntry>? entityService = null)
        where TEntry : class, IHierarchyEntry<TEntry>, new();

    /// <summary>
    /// Informs the ViewDomain that a page contains sub pages.
    /// </summary>
    /// <param name="owningPage"></param>
    /// <param name="pageContainer"></param>
    void RegisterPageContainer(IPage owningPage, IEnumerable<IPage> pageContainer);

    /// <summary>
    /// Informs the ViewDomain that a page dos not (anymore) contains sub pages.
    ///
    /// This command should use optimistic behavior: If the page is not known or is already unregistered, it should
    /// just end without throwing any exception.
    /// </summary>
    /// <param name="owningPage"></param>
    void UnregisterPageContainer(IPage owningPage);

    // messaging
    void ShowInfoMessage(string message, string? title = null, IPage? ownerPage = null);
    void ShowErrorMessage(string message, string? title = null, IPage? ownerPage = null, Exception? exception = null);
    bool ShowYesNoQuestionMessage(string message, string? title = null, IPage? ownerPage = null);

    /// <summary>
    /// A message where you can click on OK/Confirm and Cancel.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="ownerPage"></param>
    /// <returns></returns>
    bool ShowConfirmMessage(string message, string? title = null, IPage? ownerPage = null);

    // page interaction methods
    void ShowPage(IPage page);
    bool? ShowDialogPage(IPage page, IPage? ownerPage = null);
    void ClosePage(IPage page);
    void PageEndEdit(IPage page);

    // data grid methods
    /// <summary>
    /// Starts editing the current entity in the default data grid (based on the `base data set`) on the page.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="enforceFirstEditableRow"></param>
    void StartEditingDefaultDataGridCurrentEntity(IDataPage page, bool enforceFirstEditableRow);

    string? OpenOpenFileDialog(string title, string fileMask, IPage? ownerPage = null);
    string? OpenSaveFileDialog(string title, string fileMask, IPage? ownerPage = null);

    /// <summary>
    /// Opens an dialog for an report for the given model and layout
    /// </summary>
    /// <param name="model"></param>
    /// <param name="reportLayout"></param>
    /// <param name="ownerPage"></param>
    // TODO: type definition currently not possible for 'model' and 'layout'
    bool OpenReportDialog(object model, object? reportLayout = null, IPage? ownerPage = null);

    /// <summary>
    /// Opens a file with the default application for it.
    /// 
    /// When the view is an web, the file is downloaded.
    /// </summary>
    /// <param name="filename"></param>
    void OpenFile(string filename);

    /// <summary>
    /// An event handler which sends messages from a business layer object directly
    /// to the UI as single message. In a desktop environment, this could be a message box.
    /// 
    /// In a Web environment, this could be a bootstrap alert message.
    /// </summary>
    /// <param name="businessLayerObject">
    /// The business layer object throwing the message
    /// </param>
    /// <param name="eventArgs">
    /// The event with the business layer message.
    /// </param>
    void BusinessLayerMessageEventToSingleUIMessage(object? businessLayerObject, ReportBusinessLayerMessageEventArgs eventArgs);

    #region Menu functions

    void RegisterDynamicMenuItem(Type destinationType, UIMenuItem menuItem, string? menuName = null, string? menuPath = null);
    void RegisterDynamicMenuItem(string fullTypeName, UIMenuItem menuItem, string? menuName = null, string? menuPath = null);

    #endregion
}