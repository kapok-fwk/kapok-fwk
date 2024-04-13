using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Kapok.Report.SqlServer;

public class SqlReportDataSet : IDbReportDataSetAsync
{
    public string? DataSourceName { get; set; }

    /// <summary>
    /// The SQL Query to be called.
    /// </summary>
    public string? SqlQuery { get; set; }

    /// <summary>
    /// The resource name for property <see cref="SqlQuery"/>.
    /// </summary>
    public string? SqlQueryResourceName { get; set; }

    public IEnumerator GetEnumerator()
    {
        throw new NotSupportedException();
    }

    private string GetSqlQuery(IReportResourceProvider? resourceProvider = default)
    {
        string sqlQuery;
        if (SqlQueryResourceName != null)
        {
            if (resourceProvider == null)
            {
                throw new ArgumentException($"The DataSet uses a resource but {nameof(resourceProvider)} was not given", nameof(resourceProvider));
            }
            sqlQuery = System.Text.Encoding.Default.GetString(resourceProvider[SqlQueryResourceName].Data ?? Array.Empty<byte>());
        }
        else if (SqlQuery != null)
        {
            sqlQuery = SqlQuery;
        }
        else
        {
            throw new NotSupportedException($"Could not determine MDX query from DataSet. You need to set property {SqlQuery} or {SqlQueryResourceName}.");
        }
        return sqlQuery;
    }

    public void ExecuteQuery(IDbConnection connection, IReadOnlyDictionary<string, object?>? parameters = default,
        IReportResourceProvider? resourceProvider = default)
    {
        ExecuteQuery(connection, GetSqlQuery(resourceProvider), parameters);
    }

    public Task ExecuteQueryAsync(IDbConnection connection, IReadOnlyDictionary<string, object?>? parameters = default,
        IReportResourceProvider? resourceProvider = default)
    {
        return ExecuteQueryAsync(connection, GetSqlQuery(resourceProvider), parameters);
    }

    /// <summary>
    /// The result sql table from the SQL query.
    /// </summary>
    public DataSet? DataSet { get; private set; }

    private void ExecuteQuery(IDbConnection connection, string sqlQuery,
        IReadOnlyDictionary<string, object?>? parameters)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));

        var command = new SqlCommand(sqlQuery, (SqlConnection)connection);
        command.CommandTimeout = 0;  // set command timeout to infinity

        if (parameters != null)
        {
            foreach (var reportParameter in parameters)
            {
                command.Parameters.Add(new SqlParameter(reportParameter.Key, reportParameter.Value));
            }
        }

        bool handleConnection = connection.State == ConnectionState.Closed;

        try
        {
            if (handleConnection)
                connection.Open();

            var adapter = new SqlDataAdapter(command);
            var dataSet = new DataSet();
            adapter.Fill(dataSet);
            DataSet = dataSet;
        }
        finally
        {
            if (handleConnection)
                connection.Close();
        }
    }

    private async Task ExecuteQueryAsync(IDbConnection connection, string sqlQuery,
        IReadOnlyDictionary<string, object?>? parameters)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));

        var command = new SqlCommand(sqlQuery, (SqlConnection)connection);
        command.CommandTimeout = 0;  // set command timeout to infinity

        if (parameters != null)
        {
            foreach (var reportParameter in parameters)
            {
                command.Parameters.Add(new SqlParameter(reportParameter.Key, reportParameter.Value));
            }
        }

        bool handleConnection = connection.State == ConnectionState.Closed;

        if (handleConnection)
        {
            var dataSet = new DataSet();
            var dataAdapter = new SqlDataAdapter(command);
            await Task.Run(() =>
            {
                try
                {
                    connection.Open();

                    dataAdapter.Fill(dataSet);
                }
                finally
                {
                    connection.Close();
                }
            });

            DataSet = dataSet;
        }
        else
        {
            try
            {
                connection.Open();

                var dataSet = new DataSet();
                var dataAdapter = new SqlDataAdapter(command);
                await Task.Run(() => dataAdapter.Fill(dataSet));

                DataSet = dataSet;
            }
            finally
            {
                connection.Close();
            }
        }
    }
}