namespace Kapok.DataPort;

public interface IDataPort
{
    IDataPortSource Source { get; }
    IDataPortTarget Target { get; }
}
