using System.Linq.Expressions;

namespace Kapok.Report;

public class QueryableReportDataSet<T> : ReportDataSet<T>, IQueryable<T>
{
    private readonly IQueryable<T> _queryable;

    public QueryableReportDataSet(IQueryable<T> queryable)
    {
        _queryable = queryable;
    }

    public override IEnumerator<T> GetEnumerator()
    {
        return _queryable.GetEnumerator();
    }

    #region Implement IQueryable

    Type IQueryable.ElementType => _queryable.ElementType;
    Expression IQueryable.Expression => _queryable.Expression;
    IQueryProvider IQueryable.Provider => _queryable.Provider;

    #endregion
}