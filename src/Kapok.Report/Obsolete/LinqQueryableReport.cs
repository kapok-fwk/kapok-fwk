namespace Kapok.Report.Model;

/// <summary>
/// A simple linq report based upon a single given queryable object.
/// </summary>
[Obsolete]
[ReportProcessor(typeof(LinqQueryableReportProcessor))]
public class LinqQueryableReport : DataTableReport
{
    public IQueryable QueryableObject { get; set; }

    public string WhereClause { get; set; }

    public string OrderByClause { get; set; }

    public string SelectClause { get; set; }

    public int Take { get; set; }

    public string GroupByClause { get; set; }
}