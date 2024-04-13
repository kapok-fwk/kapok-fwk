using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Report.DataModel;

namespace Kapok.Report.BusinessLayer;

public interface IReportDestinationService : IEntityService<ReportDestination>
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

public class ReportDestinationService : EntityService<ReportDestination>, IReportDestinationService
{
    public ReportDestinationService(IDataDomainScope dataDomainScope, IRepository<ReportDestination> repository) : base(dataDomainScope, repository)
    {
    }

    public Stream CreateStreamInstance(ReportDestination entity)
    {
        switch (entity.Type)
        {
            case ReportDestinationType.File:
                return CreateFileStream(entity);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // ReSharper disable once UnusedParameter.Local
    private Stream CreateFileStream(ReportDestination entity)
    {
        // TODO: we should here identify if the file should be written on client or server side!!
        // TODO: here we should use JSON parsing to get to the File-ExtendedData object (entity.ExtendedData)
        throw new NotImplementedException();
    }
}