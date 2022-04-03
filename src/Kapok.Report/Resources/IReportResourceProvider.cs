namespace Kapok.Report;

/// <summary>
/// A interface defining a provider for report resources.
/// </summary>
public interface IReportResourceProvider : ICollection<ReportResource>
{
    ReportResource this[string resourceName] { get; set; }

    ReportResource? TryGet(string? resourceName);
}