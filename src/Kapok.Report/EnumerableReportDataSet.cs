namespace Kapok.Report;

public class EnumerableReportDataSet<T> : ReportDataSet<T>
{
    private readonly IEnumerable<T> _enumerable;

    public EnumerableReportDataSet(IEnumerable<T> enumerable)
    {
        _enumerable = enumerable;
    }

    public override IEnumerator<T> GetEnumerator() => _enumerable.GetEnumerator();
}