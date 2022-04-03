using System;
using System.Globalization;

namespace Kapok.DataPort;

public class TableDataPortMap
{
    public DataPortColumn? SourceColumn { get; set; }

    public DataPortColumn? TargetColumn { get; set; }

    public CultureInfo? SourceCultureInfo { get; set; }
    public string?[]? SourceFormats { get; set; }

    public CultureInfo? TargetCultureInfo { get; set; }
    public string? TargetFormat { get; set; }

    /// <summary>
    /// A custom parser of the value from the source value to the target value.
    /// </summary>
    public Func<object?, object?>? CustomMapValue { get; set; }
}