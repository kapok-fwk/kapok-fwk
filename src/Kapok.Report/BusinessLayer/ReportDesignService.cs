using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Report.DataModel;

namespace Kapok.Report.BusinessLayer;

public interface IReportDesignService : IEntityService<ReportDesign>
{
}

public class ReportDesignService : EntityService<ReportDesign>, IReportDesignService
{
    public ReportDesignService(IDataDomainScope dataDomainScope, IRepository<ReportDesign> repository) : base(dataDomainScope, repository)
    {
    }
}