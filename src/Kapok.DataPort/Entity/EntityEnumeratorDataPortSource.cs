namespace Kapok.DataPort.Entity;

public class EntityEnumeratorDataPortSource<TEntity> : EntityDataPortSourceBase<TEntity>
    where TEntity : class
{
    private IEnumerator<TEntity>? _enumerator;

    public virtual IEnumerable<TEntity>? SourceEnumerable { get; set; }

    protected override TEntity? OnRead()
    {
        if (_enumerator == null)
        {
            if (SourceEnumerable == null)
                throw new InvalidOperationException($"The property {nameof(SourceEnumerable)} has not been set before reading from the DataPortSource.");

            _enumerator = SourceEnumerable.GetEnumerator();
        }

        if (_enumerator.MoveNext())
            return _enumerator.Current;
            
        return null;
    }
}