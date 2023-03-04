namespace Kapok.BusinessLayer;

public interface INotifyFilterChanged
{
    event EventHandler FilterChanged;
}