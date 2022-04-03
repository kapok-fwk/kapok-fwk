namespace Kapok.DataPort;

public interface ITableDataPort : IDataPort
{
    new IDataPortTableSource Source { get; }
    new IDataPortTableTarget Target { get; }

    void Execute();
}