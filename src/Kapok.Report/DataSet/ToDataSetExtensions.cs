namespace Kapok.Report;

public static class ToDataSetExtensions
{
    public static EnumerableReportDataSet<T> AsReportDataSet<T>(this IEnumerable<T> enumerable)
    {
        return new EnumerableReportDataSet<T>(enumerable);
    }

    public static QueryableReportDataSet<T> AsReportDataSet<T>(this IQueryable<T> queryable)
    {
        return new QueryableReportDataSet<T>(queryable);
    }
}