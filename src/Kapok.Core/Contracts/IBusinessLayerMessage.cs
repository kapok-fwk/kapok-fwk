namespace Kapok.BusinessLayer;

public interface IBusinessLayerMessage
{
    MessageSeverity Severity { get; }
    string Text { get; }
}