using System.ComponentModel.DataAnnotations;

namespace Kapok.Report;

/// <summary>
/// A base class for a report data source.
/// </summary>
public abstract class ReportDataSource
{
    [Required]
    public string? Name { get; set; }
}