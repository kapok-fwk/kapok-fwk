namespace Kapok.Core;

public abstract class TrackingDataDomainScope : DataDomainScope
{
    private readonly ChangeTracker _changeTracker;

    protected TrackingDataDomainScope(IDataDomain dataDomain) : base(dataDomain)
    {
        _changeTracker = new ChangeTracker();
    }

    public override bool CanSave()
    {
        return _changeTracker.AnyChangesOutstanding();
    }

    public override void Save()
    {
        foreach (var tracking in _changeTracker.ToList())
        {
            switch (tracking.State)
            {
                case ChangeTrackingState.None:
                    break;
                case ChangeTrackingState.Created:
                    ExecuteCreate(tracking.EntityType, tracking.Entity);
                    tracking.State = ChangeTrackingState.None;
                    tracking.OriginalEntity = null;
                    break;
                case ChangeTrackingState.Updated:
                    ExecuteUpdate(tracking.EntityType, tracking.OriginalEntity, tracking.Entity);
                    tracking.State = ChangeTrackingState.None;
                    tracking.OriginalEntity = null;
                    break;
                case ChangeTrackingState.Deleted:
                    ExecuteDelete(tracking.EntityType, tracking.OriginalEntity ?? tracking.Entity);
                    tracking.State = ChangeTrackingState.Detached;
                    tracking.OriginalEntity = null;
                    break;
                case ChangeTrackingState.Detached:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        foreach(var tracking in _changeTracker.Where(t => t.State == ChangeTrackingState.Detached).ToList())
        {
            _changeTracker.Detach(tracking);
        }
    }

    protected abstract void ExecuteCreate(Type entityType, object entity);

    protected abstract void ExecuteUpdate(Type entityType, object? oldEntity, object newEntity);

    protected abstract void ExecuteDelete(Type entityType, object entity);

    public override Task SaveAsync(CancellationToken cancellationToken = default)
    {
        // TODO: this is not a correct async implementation
        Save();
        return Task.CompletedTask;
    }

    public override void RejectChanges()
    {
        _changeTracker.Clear();
    }
}