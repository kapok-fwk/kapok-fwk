using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Report.DataModel;

namespace Kapok.Report;

public class ReportDestinationDao : Dao<ReportDestination>, IReportDestinationDao
{
    public ReportDestinationDao(IDataDomainScope dataDomainScope) : base(dataDomainScope)
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