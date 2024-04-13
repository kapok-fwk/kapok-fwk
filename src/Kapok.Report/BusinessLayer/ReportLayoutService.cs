using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Report.DataModel;

namespace Kapok.Report.BusinessLayer;

public interface IReportLayoutService : IEntityService<ReportLayout>
{
}

public class ReportLayoutService : EntityService<ReportLayout>, IReportLayoutService
{
    public ReportLayoutService(IDataDomainScope dataDomainScope, IRepository<ReportLayout> repository) : base(dataDomainScope, repository)
    {
    }
}