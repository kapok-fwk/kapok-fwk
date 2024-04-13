using System.Data;
using Kapok.Report.Model;

namespace Kapok.Report;

/// <summary>
/// A report data set with async. functions against a database connection.
/// </summary>
public interface IDbReportDataSetAsync : IDbReportDataSet
{
    Task ExecuteQueryAsync(IDbConnection connection,
        IReadOnlyDictionary<string, object?>? parameters = default,
        IReportResourceProvider? resourceProvider = default);

    Task ExecuteQueryAsync(IDbConnection connection,
        ReportParameterCollection parameters,
        IReportResourceProvider? resourceProvider = default) =>
        ExecuteQueryAsync(connection, parameters.ToDictionary(), resourceProvider);
}