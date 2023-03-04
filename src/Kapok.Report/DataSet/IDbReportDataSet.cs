using System.Data;

namespace Kapok.Report;

/// <summary>
/// A report data set requiring a database connection.
/// </summary>
public interface IDbReportDataSet : IReportDataSet
{
    /// <summary>
    /// The name of the data source. This will be used to 
    /// </summary>
    string? DataSourceName { get; set; }

    /// <summary>
    /// Executes the query against the database.
    /// </summary>
    /// <param name="connection">
    /// The connection to be used in the query.
    /// </param>
    /// <param name="parameters">
    /// The query parameters to be passed over.
    /// </param>
    /// <param name="resourceProvider">
    /// The resource provider in case the DataSet uses resources to
    /// e.g. store the database query.
    /// </param>
    void ExecuteQuery(IDbConnection connection,
        IReadOnlyDictionary<string, object?>? parameters = default,
        IReportResourceProvider? resourceProvider = default);
}