namespace Kapok.Core;

public interface INotifyFilterChanged
{
    event EventHandler FilterChanged;
}