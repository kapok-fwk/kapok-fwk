#if DEBUG
using System.Diagnostics;
#endif
using NLog;

namespace Kapok.BusinessLayer;

public class ReportBusinessLayerMessageEventArgs : EventArgs
{
    public ReportBusinessLayerMessageEventArgs(BusinessLayerMessage message)
    {
        Message = message;
    }

    public BusinessLayerMessage Message { get; }
}

public abstract class BusinessLayerBase
{
    protected static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

    // TODO: this is only a temporary solution
    public bool ThrowOnError { get; set; }

    public event EventHandler<ReportBusinessLayerMessageEventArgs>? ReportMessage;

    private void RaiseReportMessage(MessageSeverity messageSeverity, string message)
    {
        Report(new BusinessLayerMessage(message, messageSeverity));
    }

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

        ReportMessage?.Invoke(this,
            new ReportBusinessLayerMessageEventArgs(businessLayerMessage));
    }

    protected void Report(IEnumerable<BusinessLayerMessage> businessLayerMessages)
    {
        foreach (var businessLayerMessage in businessLayerMessages)
        {
            Report(businessLayerMessage);
        }
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
        IsErrorReported = true;
        RaiseReportMessage(MessageSeverity.Error, message);
        if (ThrowOnError)
            throw new BusinessLayerErrorException(message);
    }

    protected void ReportError(string message, params object[] args)
    {
        ReportError(string.Format(message, args));
    }

    protected bool IsErrorReported { get; private set; }
    protected void ResetReportedError() { IsErrorReported = false; }
}