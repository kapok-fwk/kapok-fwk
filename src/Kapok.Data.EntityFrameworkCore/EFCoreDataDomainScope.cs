using Kapok.Data.InMemory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kapok.Data.EntityFrameworkCore;

public sealed class EFCoreDataDomainScope : DataDomainScope
{
    private readonly IEntityFrameworkCoreDataDomain _dataDomain;
    private DbContext? _dbContext;

    /// <summary>
    /// The lock object used when the DbContext is saved, rejected or just replaced.
    /// </summary>
    private readonly object _dbContextLockObject = new();

    public EFCoreDataDomainScope(IEntityFrameworkCoreDataDomain dataDomain)
        : base(dataDomain)
    {
        _dataDomain = dataDomain ?? throw new ArgumentNullException(nameof(dataDomain));
    }

    // ReSharper disable once InconsistentlySynchronizedField
    internal DbContext? DbContext => IsDisposed ? null : _dbContext ??= _dataDomain.ConstructNewDbContext();

    public IModel? Model => DbContext?.Model ?? null;

    protected override void OnDispose()
    {
        lock (_dbContextLockObject)
        {
            _dbContext?.Dispose();
        }
    }

    public override bool CanSave()
    {
        if (IsDisposed)
            return false;

        if (CanSaveAnyDeferredCommitDao())
            return true;

        return false;
    }

    public override void Save()
    {
        CheckIsDisposed();

        lock (_dbContextLockObject)
        {
            SaveDeferredCommitDao();

            if (_dbContext == null) // when nothing was saved to the repositories, the DbContext will not be initialized
                return;

            _dbContext.SaveChanges();

            _dbContext.ChangeTracker.Clear(); // enforce to unbind from all entities
            _dbContext.Dispose();
            _dbContext = null;

            PostSaveDeferredCommitDao();
        }
    }

    public override async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        CheckIsDisposed();

        ValueTask task;

        lock (_dbContextLockObject)
        {
            SaveDeferredCommitDao();

            if (_dbContext == null) // when nothing was saved to the repositories, the DbContext will not be initialized
                return;

            // TODO: ends with a crash when using the SaveChangesAsync async method, so we call int synchronous
            //await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.SaveChanges();

            _dbContext.ChangeTracker.Clear(); // enforce to unbind from all entities
            task = _dbContext.DisposeAsync();
            _dbContext = null;

            PostSaveDeferredCommitDao();
        }

        await task;
    }

    public override void RejectChanges()
    {
        CheckIsDisposed();

        lock (_dbContextLockObject)
        {
            if (_dbContext != null)
            {
                // this should not be necessary but just in case, we enforce to unbind all tracked entries
                // before resetting the entries
                _dbContext.ChangeTracker.Clear(); // enforce to unbind from all entities
                _dbContext.Dispose();
                _dbContext = null;
            }

            RejectChangesDeferredCommitDao();
        }
    }

    protected override IRepository<T> InitializeRepository<T>()
    {
        CheckIsDisposed();

#pragma warning disable 8602
        var entityType = DbContext.Model.FindEntityType(typeof(T));
#pragma warning restore 8602
        if (entityType != null)
            return new EFCoreRepository<T>(this);

        // when the repository is not part of the DbContext, it is an
        // virtual entity so we just return an in-memory repository
        return new InMemoryRepository<T>();
    }
}