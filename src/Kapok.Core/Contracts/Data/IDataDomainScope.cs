using System.Transactions;
using Kapok.Data;

namespace Kapok.Core;

public interface IDataDomainScope : IDisposable
{
    IDataDomain DataDomain { get; }

    IReadOnlyDictionary<string, DataPartition> DataPartitions { get; }

    bool CanSave();
    void Save();
    Task SaveAsync(CancellationToken cancellationToken = default);
    void RejectChanges();

    /// <summary>
    /// Adds a DAO for an entity. If an DAO for the entity is already added, the
    /// method will through an ArgumentE exception.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="dao"></param>
    void AddDao<TEntity>(IDao<TEntity> dao)
        where TEntity : class, new();

    /// <summary>
    /// Gets a DAO from the data domain scope. If not added, use the default initialization for the entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    IDao<TEntity> GetDao<TEntity>()
        where TEntity : class, new();

    TDao GetDao<TEntity, TDao>()
        where TEntity : class, new()
        where TDao : IDao<TEntity>;

    ITransactionScope BeginTransaction()
        => BeginTransaction(IsolationLevel.ReadUncommitted);
    ITransactionScope BeginTransaction(IsolationLevel isolationLevel);
    void CommitTransaction();
    void RejectTransaction();
    int TransactionLevel { get; }
}