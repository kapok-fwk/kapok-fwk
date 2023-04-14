using System.Transactions;
using Kapok.BusinessLayer;
using Res = Kapok.Resources.Data.DataDomainScope;

namespace Kapok.Data;

public abstract class DataDomainScope : IDataDomainScope
{
    private readonly List<TransactionScope> _transactionScopes = new();
    private readonly Dictionary<Type, object> _daos = new();
    private readonly Dictionary<Type, object> _repositories = new();
    private readonly List<IDeferredCommitDao> _deferredCommitDao = new();

    protected DataDomainScope(IDataDomain dataDomain)
    {
        DataDomain = dataDomain ?? throw new ArgumentNullException(nameof(dataDomain));
        DataPartitions = dataDomain.DataPartitions;
    }

    #region Deferred commit Dao handling

    protected bool CanSaveAnyDeferredCommitDao()
    {
        return _deferredCommitDao.Any(r => r.CanSave());
    }

    protected void SaveDeferredCommitDao()
    {
        foreach (var dao in _deferredCommitDao)
        {
            if (dao.CanSave())
                dao.Save();
        }
    }

    protected void RejectChangesDeferredCommitDao()
    {
        foreach (var dao in _deferredCommitDao)
        {
            if (dao.CanSave())
                dao.RejectChanges();
        }
    }

    protected void PostSaveDeferredCommitDao()
    {
        foreach (var dao in _deferredCommitDao)
        {
            dao.PostSave();
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

        _daos.Clear(); // maybe not necessary;
        _repositories.Clear(); // maybe not necessary;
        _deferredCommitDao.Clear(); // maybe not necessary;
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
        if (_repositories.ContainsKey(typeof(T)))
            return (IRepository<T>)_repositories[typeof(T)];

        var newRepository = InitializeRepository<T>();
        _repositories.Add(typeof(T), newRepository);
        return newRepository;
    }

    protected abstract IRepository<T> InitializeRepository<T>()
        where T : class, new();

    private IDao<T> InitializeDao<T>(IRepository<T> repository)
        where T : class, new()
    {
        return Kapok.Data.DataDomain.ConstructNewDao(this, repository);
    }

    private void AddDaoInternal<T>(IDao<T> dao)
        where T : class, new()
    {
        _daos.Add(typeof(T), dao);
        if (dao is IDeferredCommitDao deferredCommitDao)
            _deferredCommitDao.Add(deferredCommitDao);
    }

    public void AddDao<T>(IDao<T> dao)
        where T : class, new()
    {
        if (dao == null) throw new ArgumentNullException(nameof(dao));
        CheckIsDisposed();
        var entityType = typeof(T);

        if (_daos.ContainsKey(entityType))
            throw new ArgumentException(string.Format(Res.DaoAlreadyAdded, entityType.FullName));

        AddDaoInternal(dao);
    }

    public IDao<T> GetDao<T>()
        where T : class, new()
    {
        CheckIsDisposed();
        var entityType = typeof(T);

        if (_daos.ContainsKey(entityType))
            return (IDao<T>)_daos[entityType];

        IDao<T> newDao = InitializeDao(GetRepository<T>());
        AddDaoInternal(newDao);
        return newDao;
    }

    public TRepository GetDao<TEntity, TRepository>()
        where TEntity : class, new()
        where TRepository : IDao<TEntity>
    {
        return (TRepository) GetDao<TEntity>();
    }

    #region IDataDomainScope not implemented methods

    public abstract bool CanSave();
    public abstract void Save();
    public abstract Task SaveAsync(CancellationToken cancellationToken = default);
    public abstract void RejectChanges();

    #endregion
}