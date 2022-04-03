using System;
using System.Collections.Generic;

namespace Kapok.DataPort;

public class DataPortEntityEnumeratorSource<TEntity> : EntityDataPortSource<TEntity>
    where TEntity : class
{
    private IEnumerator<TEntity>? _enumerator;

    public IEnumerable<TEntity>? SourceEnumerable { get; set; }

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