using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kapok.Data.EntityFrameworkCore;

public sealed class EFCoreDataDomainScope : DataDomainScope
{
    private DbContext? _dbContext;

    /// <summary>
    /// The lock object used when the DbContext is saved, rejected or just replaced.
    /// </summary>
    private readonly object _dbContextLockObject = new();

    public EFCoreDataDomainScope(IDataDomain dataDomain, IServiceProvider serviceProvider)
        : base(dataDomain, serviceProvider)
    {
    }

    // ReSharper disable once InconsistentlySynchronizedField
    internal DbContext? DbContext => IsDisposed ? null : _dbContext ??= ((IEntityFrameworkCoreDataDomain)DataDomain).ConstructNewDbContext();

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

        if (CanSaveAnyDeferredCommit())
            return true;

        return false;
    }

    public override void Save()
    {
        CheckIsDisposed();

        lock (_dbContextLockObject)
        {
            SaveDeferredCommit();

            if (_dbContext == null) // when nothing was saved to the repositories, the DbContext will not be initialized
                return;

            _dbContext.SaveChanges();

            // Unbind from all entities, but keep the DbContext itself alive so that a subsequent
            // Save() within the same scope can reuse it instead of paying for disposing and lazily
            // re-constructing a brand new DbContext (and its internal service provider) on every Save().
            // The DbContext is only actually disposed when the scope itself is disposed (see OnDispose).
            _dbContext.ChangeTracker.Clear();

            PostSaveDeferredCommit();
        }
    }

    public override Task SaveAsync(CancellationToken cancellationToken = default)
    {
        CheckIsDisposed();

        lock (_dbContextLockObject)
        {
            SaveDeferredCommit();

            if (_dbContext == null) // when nothing was saved to the repositories, the DbContext will not be initialized
                return Task.CompletedTask;

            // TODO: ends with a crash when using the SaveChangesAsync async method, so we call int synchronous
            //await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.SaveChanges();

            // see the comment in Save() for why we clear instead of dispose+recreate here
            _dbContext.ChangeTracker.Clear();

            PostSaveDeferredCommit();
        }

        return Task.CompletedTask;
    }

    public override void RejectChanges()
    {
        CheckIsDisposed();

        lock (_dbContextLockObject)
        {
            if (_dbContext != null)
            {
                // this should not be necessary but just in case, we enforce to unbind all tracked entries
                // before resetting the entries. The DbContext itself is kept alive for reuse within this
                // scope, same as in Save() -- see the comment there.
                _dbContext.ChangeTracker.Clear();
            }

            RejectChangesDeferredCommitService();
        }
    }

    /*protected override IRepository<T> InitializeRepository<T>()
    {
        CheckIsDisposed();

#pragma warning disable 8602
        var entityType = DbContext.Model.FindEntityType(typeof(T));
#pragma warning restore 8602
        if (entityType != null)
            return new EFCoreRepository<T>(this);

        // when the repository is not part of the DbContext, it is an
        // virtual entity so we just return an in-memory repository with the lifetime of scope
        return new InMemoryRepository<T>();
    }*/
}