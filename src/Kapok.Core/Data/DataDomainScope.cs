using System.Transactions;
using Kapok.BusinessLayer;
using Kapok.Data.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Res = Kapok.Resources.Data.DataDomainScope;

namespace Kapok.Data;

public abstract class DataDomainScope : IDataDomainScope
{
    public IServiceProvider ServiceProvider { get; }
    private readonly List<TransactionScope> _transactionScopes = new();
    private readonly Dictionary<Type, object> _entityServices = new();
    private readonly Dictionary<Type, object> _repositories = new();
    private readonly List<IEntityDeferredCommitService> _entityDeferredCommitServices = new();

    protected DataDomainScope(IDataDomain dataDomain, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(dataDomain, nameof(dataDomain));
        DataDomain = dataDomain;
        DataPartitions = dataDomain.DataPartitions;
        ServiceProvider = serviceProvider;
    }

    #region Deferred commit entity service handling

    protected bool CanSaveAnyDeferredCommit()
    {
        return _entityDeferredCommitServices.Any(r => r.CanSave());
    }

    protected void SaveDeferredCommit()
    {
        foreach (var entityService in _entityDeferredCommitServices)
        {
            if (entityService.CanSave())
                entityService.Save();
        }
    }

    protected void RejectChangesDeferredCommitService()
    {
        foreach (var entityService in _entityDeferredCommitServices)
        {
            if (entityService.CanSave())
                entityService.RejectChanges();
        }
    }

    protected void PostSaveDeferredCommit()
    {
        foreach (var entityService in _entityDeferredCommitServices)
        {
            entityService.PostSave();
        }
    }

    #endregion

    #region Transaction handling

    private class TransactionWrapper : ITransactionScope
    {
        private readonly TransactionScope _scope;
        private readonly DataDomainScope _dataDomainScope;
        private bool _isCommitted;

        public TransactionWrapper(TransactionScope scope, DataDomainScope dataDomainScope)
        {
            _scope = scope;
            _dataDomainScope = dataDomainScope;
        }

        public void Commit()
        {
            _scope.Complete();
            _dataDomainScope._transactionScopes.Remove(_scope);
            _isCommitted = true;
        }

        public void Dispose()
        {
            _scope.Dispose();
            if (!_isCommitted)
                _dataDomainScope._transactionScopes.Remove(_scope);
        }
    }

    public int TransactionLevel => _transactionScopes.Count;

    public ITransactionScope BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted)
    {
        var options = new TransactionOptions
        {
            IsolationLevel = isolationLevel,
            Timeout = TimeSpan.MaxValue
        };

        var newScope = new TransactionScope(TransactionScopeOption.Required, options);
        _transactionScopes.Add(newScope);
        return new TransactionWrapper(newScope, this);
    }

    public void CommitTransaction()
    {
        if (_transactionScopes.Count == 0)
            throw new NotSupportedException(Res.NoTransactionToCommit);

        var lastTransactionScope = _transactionScopes.Last();
        lastTransactionScope.Complete();
        lastTransactionScope.Dispose();
        _transactionScopes.Remove(lastTransactionScope);
    }

    public void RejectTransaction()
    {
        if (_transactionScopes.Count == 0)
            throw new NotSupportedException(Res.NoTransactionToCommit);

        var lastTransactionScope = _transactionScopes.Last();
        lastTransactionScope.Dispose();
        _transactionScopes.Remove(lastTransactionScope);
    }

    #endregion

    protected bool IsDisposed { get; private set; }

    protected void CheckIsDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(DataDomainScope));
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        foreach (var transactionScope in _transactionScopes)
        {
            transactionScope.Dispose();
        }

        _entityServices.Clear(); // maybe not necessary;
        _repositories.Clear(); // maybe not necessary;
        _entityDeferredCommitServices.Clear(); // maybe not necessary;
        OnDispose();

        IsDisposed = true;
    }

    protected virtual void OnDispose()
    {
    }

    public IDataDomain DataDomain { get; }

    public IReadOnlyDictionary<string, DataPartition> DataPartitions { get; }

    public IRepository<T> GetRepository<T>()
        where T : class, new()
    {
        CheckIsDisposed();

        if (Data.DataDomain.Entities.TryGetValue(typeof(T), out var registeredEntity))
        {
            if (registeredEntity.IsVirtual)
            {
                // return in memory repository to cache virtually the data
                return new InMemoryRepository<T>();
            }
        }

        return ServiceProvider.GetRequiredService<IRepository<T>>();
    }

    private IEntityService<T> InitializeEntityService<T>(IRepository<T> repository)
        where T : class, new()
    {
        var entityType = typeof(T);

        if (!Data.DataDomain.Entities.ContainsKey(entityType))
            throw new ArgumentException(
                $"The passed generic type {typeof(T).FullName} is not registered as entity. The entity service object cannot be created.");

        var registeredEntity = Data.DataDomain.Entities[entityType];

        var entityService = (IEntityService<T>?)ServiceProvider.GetService(registeredEntity.EntityServiceType);
        if (entityService != null) return entityService;

        if (registeredEntity.ContractType != null)
        {
            entityService = (IEntityService<T>?)ServiceProvider.GetService(registeredEntity.ContractType);
            if (entityService != null) return entityService;
        }

        entityService = (IEntityService<T>)ActivatorUtilities.CreateInstance(ServiceProvider, registeredEntity.EntityServiceType, 
            this, repository);

        return entityService;
    }

    private void AddEntityServiceInternal<T>(IEntityService<T> entityService)
        where T : class, new()
    {
        _entityServices.Add(typeof(T), entityService);
        if (entityService is IEntityDeferredCommitService deferredCommitService)
            _entityDeferredCommitServices.Add(deferredCommitService);
    }

    public void AddEntityService<T>(IEntityService<T> entityService)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(entityService, nameof(entityService));
        CheckIsDisposed();
        var entityType = typeof(T);

        if (_entityServices.ContainsKey(entityType))
            throw new ArgumentException(string.Format(Res.EntityServiceAlreadyAdded, entityType.FullName));

        AddEntityServiceInternal(entityService);
    }

    public IEntityService<T> GetEntityService<T>()
        where T : class, new()
    {
        CheckIsDisposed();
        var entityType = typeof(T);

        if (_entityServices.TryGetValue(entityType, out var entityService))
            return (IEntityService<T>)entityService;

        IEntityService<T> newEntityService = InitializeEntityService(GetRepository<T>());
        AddEntityServiceInternal(newEntityService);
        return newEntityService;
    }

    public TRepository GetEntityService<TEntity, TRepository>()
        where TEntity : class, new()
        where TRepository : IEntityService<TEntity>
    {
        return (TRepository) GetEntityService<TEntity>();
    }

    #region IDataDomainScope not implemented methods

    public abstract bool CanSave();
    public abstract void Save();
    public abstract Task SaveAsync(CancellationToken cancellationToken = default);
    public abstract void RejectChanges();

    #endregion
}