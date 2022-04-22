namespace Kapok.DataPort.Entity;

public class EntityCollectionDataPortTarget<TEntity> : EntityDataPortTargetBase<TEntity>
    where TEntity : class, new()
{
    public ICollection<TEntity>? TargetCollection { get; set; }

    protected override void OnWrite(TEntity entity)
    {
        if (TargetCollection == null)
            throw new InvalidOperationException($"The property {nameof(TargetCollection)} has not been set before writing into the DataPortTarget.");

        TargetCollection.Add(entity);
    }
}