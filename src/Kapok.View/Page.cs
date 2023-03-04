using System.ComponentModel;
using Kapok.BusinessLayer;
#if DEBUG
using System.Diagnostics;
#endif
using NLog;

namespace Kapok.View;

/// <summary>
/// A base class for all pages
/// </summary>
public abstract class Page : ValidatableBindableObjectBase, IPage
{
    protected static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

    private string? _title;
    private bool _canClose;

#pragma warning disable CS8618
    protected Page(IViewDomain? viewDomain = null)
#pragma warning restore CS8618
    {
        if (viewDomain == null && View.ViewDomain.Default == null)
            throw new NotSupportedException(
                $"You have to first set Kapok.View.ViewDomain.Default before you can initiate a page without {nameof(viewDomain)} being provided");
#pragma warning disable CS8601
        ViewDomain = viewDomain ?? View.ViewDomain.Default;
#pragma warning restore CS8601
        Title = GetType().ToString();
        _canClose = true;

        OnLoadingAction = new UIAction("OnLoadingPage", OnLoadingInternal);
        OnLoadedAction = new UIAction("OnLoadedPage", OnLoadedInternal);
        CloseAction = new UIAction("ClosePage", Close);
    }

    #region Properties

    public IViewDomain ViewDomain { get; }

    public string? Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
        
    // TODO: the implementation of `CanClose` is not clear, design-review required
    public bool CanClose
    {
        get => _canClose;
        set => SetProperty(ref _canClose, value);
    }

    #endregion

    public virtual void Show()
    {
        Logger.Info("Open page {page}", GetType().Name);
        ViewDomain.ShowPage(this);
    }

    public virtual bool? ShowDialog()
    {
        Logger.Info("Open dialog page {page}", GetType().Name);
        return ViewDomain.ShowDialogPage(this);
    }

    public virtual bool? ShowDialog(IPage owner)
    {
        Logger.Info("Open dialog page {page}; {pageOwner}", GetType().Name, owner.GetType().Name);
        return ViewDomain.ShowDialogPage(this, owner);
    }

    public IAction CloseAction { get; }
        
    public virtual void Close()
    {
        Logger.Info("Close page {page}", GetType().Name);
        ViewDomain.ClosePage(this);
    }

    public event EventHandler? Closed;

    #region internal logic
        
    public IAction OnLoadingAction { get; }
        
    // TODO: rethink if 'On...' actions should be an action or if we should route them somehow else to the view model on creation
    public IAction OnLoadedAction { get; }

    private bool _isLoading;

    private void OnLoadingInternal()
    {
        if (_isLoading)
            return;

        OnLoading();
        _isLoading = true;
    }

    private bool _isLoaded;

    private void OnLoadedInternal()
    {
        // Prevent double-loading which happens when the WPF user control is used in the avalon-dock
        // Note: This is an ugly solution, because this logic is implemented in the general view layer
        //       which will be used e.g. by an REST-API or Web view domain as well.
        //
        //       I count this here as an self-protection in code, it does not cost much, tho..
        if (_isLoaded)
            return;

        if (!_isLoading)
            // Makes sure that the Loading event is always called before the Load event
            OnLoadingInternal();

        _isLoaded = true;
        OnLoaded();
    }

    private void OnClosedInternal()
    {
        _isLoaded = false;
        OnClosed();
    }

    protected virtual void OnLoading()
    {
    }

    protected virtual void OnLoaded()
    {
        Logger.Debug("Page {page} event OnLoaded", GetType().Name);
    }

    protected virtual void OnClosing(CancelEventArgs eventArgs)
    {
    }

    protected virtual void OnClosed()
    {
    }

    // TODO: I don't like that this method is public
    public void RaiseClosing(CancelEventArgs eventArgs)
    {
        OnClosing(eventArgs);
    }

    // TODO: I don't like that this method is public
    public void RaiseClosed()
    {
        OnClosedInternal();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region report and logging

    protected void Report(BusinessLayerMessage businessLayerMessage)
    {
        LogLevel nLogLevel;
        switch (businessLayerMessage.Severity)
        {
            case MessageSeverity.Debug:
                nLogLevel = LogLevel.Debug;
                break;
            case MessageSeverity.Info:
                nLogLevel = LogLevel.Info;
                break;
            case MessageSeverity.Warning:
                nLogLevel = LogLevel.Warn;
                break;
            case MessageSeverity.Error:
                nLogLevel = LogLevel.Error;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(businessLayerMessage.Severity), businessLayerMessage.Severity, null);
        }

        Logger.Log(nLogLevel, businessLayerMessage.Severity);

#if DEBUG
            Debug.WriteLine($"{GetType().Name}: {businessLayerMessage.Severity}: {businessLayerMessage.Text}");
#endif

        if (businessLayerMessage.Severity == MessageSeverity.Info ||
            businessLayerMessage.Severity == MessageSeverity.Warning)
        {
            // NOTE/TODO: we don't have here an option yet to show warning messages somehow special e.g. with an warning icon in the UI!
            ViewDomain.ShowInfoMessage(businessLayerMessage.Text, businessLayerMessage.Severity.ToDisplayName(ViewDomain.Culture));
        }
            
        if (businessLayerMessage.Severity == MessageSeverity.Error)
        {
            // TODO: use another exception class here
            throw new Exception(businessLayerMessage.Text);
        }
    }

    protected void Report(IEnumerable<BusinessLayerMessage> businessLayerMessages)
    {
        foreach (var businessLayerMessage in businessLayerMessages)
        {
            Report(businessLayerMessage);
        }
    }

    private void RaiseReportMessage(MessageSeverity messageSeverity, string message)
    {
        Report(new BusinessLayerMessage(message, messageSeverity));
    }

    protected void ReportDebug(string message)
    {
        RaiseReportMessage(MessageSeverity.Debug, message);
    }

    protected void ReportDebug(string message, params object[] args)
    {
        ReportDebug(string.Format(message, args));
    }

    protected void ReportInfo(string message)
    {
        RaiseReportMessage(MessageSeverity.Info, message);
    }

    protected void ReportInfo(string message, params object[] args)
    {
        ReportInfo(string.Format(message, args));
    }

    protected void ReportWarning(string message)
    {
        RaiseReportMessage(MessageSeverity.Warning, message);
    }

    protected void ReportWarning(string message, params object[] args)
    {
        ReportWarning(string.Format(message, args));
    }

    protected void ReportError(string message)
    {
        RaiseReportMessage(MessageSeverity.Error, message);
    }

    protected void ReportError(string message, params object[] args)
    {
        ReportError(string.Format(message, args));
    }

    #endregion
}