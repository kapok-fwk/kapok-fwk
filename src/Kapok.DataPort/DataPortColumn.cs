using System;

namespace Kapok.DataPort;

/// <summary>
/// Representing a column in a data port schema.
/// </summary>
public class DataPortColumn
{
    /// <summary>
    /// The technical name of the column.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The display name of the column.
    /// </summary>
    public Caption? DisplayName { get; set; }

    /// <summary>
    /// The display description for the column.
    /// </summary>
    public Caption? DisplayDescription { get; set; }

    /// <summary>
    /// The .NET type of the column.
    /// </summary>
    public Type? Type { get; set; }

    /// <summary>
    /// If the column is required.
    /// 
    /// This is equal to the 'Required' attribute in .NET.
    /// When required, in SQL this means 'NOT NULL', otherwise 'NULL' (a nullable column).
    /// </summary>
    public bool Required { get; set; }
}