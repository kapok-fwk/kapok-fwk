using System.Collections.Generic;

namespace Kapok.DataPort;

/// <summary>
/// A table-like data destination
/// </summary>
public interface IDataPortTableTarget : IDataPortTarget
{
    IReadOnlyList<DataPortColumn>? Schema { get; }

    void Write(Dictionary<DataPortColumn, object?> rowValues);
}