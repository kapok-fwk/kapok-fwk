using System;
using System.Collections.Generic;

namespace Kapok.DataPort;

/// <summary>
/// A table-like data source
/// </summary>
public interface IDataPortTableSource : IDataPortSource, IEnumerable<object?[]>
{
    /// <summary>
    /// If the data port provides information about the schema.
    /// </summary>
    bool HasSchema { get; }

    /// <summary>
    /// Reads the schema of the data source.
    /// </summary>
    /// <returns>
    /// Returns a list of columns of the data source.
    /// </returns>
    List<DataPortColumn>? ReadSchema();

    /// <summary>
    /// Reads the next row with all columns.
    /// </summary>
    /// <returns></returns>
    [Obsolete($"Use the {nameof(GetEnumerator)}() method")]
    object?[]? ReadNextRow();
}