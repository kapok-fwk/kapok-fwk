using System.Transactions;
using Kapok.BusinessLayer;

namespace Kapok.Data;

public interface IDataDomainScope : IDisposable
{
    IServiceProvider ServiceProvider { get; }
    IDataDomain DataDomain { get; }

    IReadOnlyDictionary<string, DataPartition> DataPartitions { get; }

    bool CanSave();
    void Save();
    Task SaveAsync(CancellationToken cancellationToken = default);
    void RejectChanges();

    /// <summary>
    /// Adds a service for an entity. If an entity service for the entity is already added, the
    /// method will through an ArgumentE exception.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityService"></param>
    void AddEntityService<TEntity>(IEntityService<TEntity> entityService)
        where TEntity : class, new();

    /// <summary>
    /// Gets an entity service from the data domain scope. If not added, use the default initialization for the entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    IEntityService<TEntity> GetEntityService<TEntity>()
        where TEntity : class, new();

    TService GetEntityService<TEntity, TService>()
        where TEntity : class, new()
        where TService : IEntityService<TEntity>;

    ITransactionScope BeginTransaction()
        => BeginTransaction(IsolationLevel.ReadUncommitted);
    ITransactionScope BeginTransaction(IsolationLevel isolationLevel);
    void CommitTransaction();
    void RejectTransaction();
    int TransactionLevel { get; }
}