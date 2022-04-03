using System.Data;

namespace Kapok.Report;

/// <summary>
/// A base report data source for a database based on IDbConnection.
/// </summary>
public abstract class DbReportDataSource : ReportDataSource
{
    public string? ConnectionString { get; set; }

    public abstract IDbConnection CreateNewConnection();
}