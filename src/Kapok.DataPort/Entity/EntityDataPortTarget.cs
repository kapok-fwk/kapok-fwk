using System.Collections.Generic;
using System.Linq;

namespace Kapok.DataPort;

public abstract class EntityDataPortTarget<TEntity> : IDataPortTableTarget
    where TEntity : class, new()
{
    private List<DataPortColumn>? _schema;

    public string Name => $"Entity target for {typeof(TEntity).FullName}";

    public IReadOnlyList<DataPortColumn> Schema => _schema ??= EntityDataPortHelper.ReadSchema(typeof(TEntity));

    public void Write(Dictionary<DataPortColumn, object?> rowValues)
    {
        var newEntity = new TEntity();

        foreach (var column in Schema.Cast<DataPortPropertyColumn>())
        {
            if (rowValues.ContainsKey(column))
            {
                column.PropertyInfo.SetMethod.Invoke(newEntity, new[]
                {
                    rowValues[column]
                });
            }
        }

        OnWrite(newEntity);
    }

    protected abstract void OnWrite(TEntity entity);
}