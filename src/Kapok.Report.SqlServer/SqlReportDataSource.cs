using System.Data;
using Microsoft.Data.SqlClient;

namespace Kapok.Report.SqlServer;

/// <summary>
/// A SQL Server report data source.
/// </summary>
public class SqlServerReportDataSource : DbReportDataSource
{
    public override IDbConnection CreateNewConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}