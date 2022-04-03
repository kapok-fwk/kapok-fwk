namespace Kapok.Data;

public interface ITransactionScope : IDisposable
{
    void Commit();
}