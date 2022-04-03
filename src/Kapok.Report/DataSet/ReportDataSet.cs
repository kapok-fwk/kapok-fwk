using System.Collections;

namespace Kapok.Report;

public abstract class ReportDataSet<T> : IDataTableReportDataSet, IEnumerable<T>
{
    /// <summary>
    /// Returns the iterated objects
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerator<T> GetEnumerator();

    #region IEnumerable

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}