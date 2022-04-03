namespace Kapok.Core;

public interface IBusinessLayerMessage
{
    MessageSeverity Severity { get; }
    string Text { get; }
}