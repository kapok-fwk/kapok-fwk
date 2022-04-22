using System.Globalization;
using Kapok.Core;
using Kapok.Entity;

namespace Kapok.View;

public interface IViewDomain
{
    CultureInfo Culture { get; }

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

    IPage ConstructPage(Type pageType, Dictionary<Type, object?>? constructorParamValues);
    TPage ConstructPage<TPage>(IDataDomainScope? dataDomainScope = null)
        where TPage : IPage;
    IDataPage ConstructEntityDefaultPage(Type entityType, IDataDomainScope? dataDomainScope = null);

    IQueryableView<TEntity> CreateQueryableView<TEntity>(IQueryable<TEntity> queryable)
        where TEntity : class;

    IPropertyLookupView? CreatePropertyLookupView(ILookupDefinition lookupDefinition, IDataDomain dataDomain, IDataSetView? dataSet = null);
    IDataSetView<TEntry> CreateDataSetView<TEntry>(IDataDomainScope dataDomainScope, IDao<TEntry>? dao = null)
        where TEntry : class, new();
    IHierarchyDataSetView<TEntry> CreateHierarchyDataSetView<TEntry>(IDataDomainScope dataDomainScope, IDao<TEntry>? dao = null)
        where TEntry : class, IHierarchyEntry<TEntry>, new();

    void RegisterPageContainer(IPage owningPage, ICollection<IPage> pageContainer);
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
    /// <param name="dataDomain"></param>
    /// <param name="reportLayout"></param>
    /// <param name="ownerPage"></param>
    // TODO: type definition currently not possible for 'model' and 'layout'
    bool OpenReportDialog(object model, IDataDomain dataDomain, object? reportLayout = null, IPage? ownerPage = null);

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
    void BusinessLayerMessageEventToSingleUIMessage(object businessLayerObject, ReportBusinessLayerMessageEventArgs eventArgs);

    #region Menu functions

    void RegisterDynamicMenuItem(Type destinationType, UIMenuItem menuItem, string? menuName = null, string? menuPath = null);
    void RegisterDynamicMenuItem(string fullTypeName, UIMenuItem menuItem, string? menuName = null, string? menuPath = null);

    #endregion
}