namespace Kapok.Core;

public class BusinessLayerMessage : IBusinessLayerMessage
{
    public BusinessLayerMessage() { }

    public BusinessLayerMessage(string text, MessageSeverity severity)
    {
        Text = text;
        Severity = severity;
    }

    public MessageSeverity Severity { get; set; }

    public string Text { get; set; }

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

    MessageSeverity IBusinessLayerMessage.Severity => MessageSeverity.Error;

    string IBusinessLayerMessage.Text => Message;
}