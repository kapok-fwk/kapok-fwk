using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kapok.DataPort;

public abstract class EntityDataPortSource<TEntity> : IDataPortTableSource
    where TEntity : class
{
    private List<DataPortColumn>? _schema;

    public string Name => $"Entity source for {typeof(TEntity).FullName}";
    public bool HasSchema => true;

    protected IReadOnlyList<DataPortColumn> Schema => _schema ??= EntityDataPortHelper.ReadSchema(typeof(TEntity));

    public List<DataPortColumn> ReadSchema()
    {
        return Schema.ToList(); // we send back a clone to make sure that no one modifies our internal schema list.
    }

    public object?[]? ReadNextRow()
    {
        if (_schema == null) throw new InvalidOperationException($"You need to call {nameof(ReadSchema)}() before calling {nameof(ReadNextRow)}()");

        var newEntity = OnRead();

        if (newEntity == null)
            return null;

        var newRow = new object?[_schema.Count];

        var columnIndex = 0;
        foreach (var column in _schema.Cast<DataPortPropertyColumn>())
        {
            newRow[columnIndex++] = column.PropertyInfo.GetMethod.Invoke(newEntity, Array.Empty<object>());
        }

        return newRow;
    }

    protected abstract TEntity? OnRead();

    public IEnumerator<object?[]> GetEnumerator()
    {
        ReadSchema();

        while (true)
        {
            object?[]? row = ReadNextRow();
            if (row == null)
                yield break;

            yield return row;
        }
    }

    #region IEnumerator

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}