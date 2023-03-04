using Kapok.BusinessLayer;
using Kapok.Report.DataModel;

namespace Kapok.Report;

public interface IReportDestinationDao : IDao<ReportDestination>
{
    /// <summary>
    /// Creates an stream which can be used to write into the destination.
    ///
    /// When the stream object is disposed, it is assumed that the processing
    /// of the report is done.
    /// </summary>
    /// <returns>
    /// Returns a stream object which is usable by an ReportProcessor.
    /// </returns>
    Stream CreateStreamInstance(ReportDestination entity);
}