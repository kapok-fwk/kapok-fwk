namespace Kapok.BusinessLayer;

public class BusinessLayerMessage : IBusinessLayerMessage
{
    public BusinessLayerMessage(string text, MessageSeverity severity)
    {
        Text = text;
        Severity = severity;
    }

    public MessageSeverity Severity { get; protected set; }

    public string Text { get; protected set; }

    public override string ToString()
    {
        return $"{Severity.ToDisplayName()}: {Text}";
    }
}

public class Error : Exception, IBusinessLayerMessage
{
    public Error(string message)
        : base(message)
    {
    }

    #region IBusinessLayerMessage

    MessageSeverity IBusinessLayerMessage.Severity => MessageSeverity.Error;

    string IBusinessLayerMessage.Text => Message;

    #endregion
}