using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Report.DataModel;

namespace Kapok.Report.BusinessLayer;

public interface IReportLayoutDao : IDao<ReportLayout>
{
}

public class ReportLayoutDao : Dao<ReportLayout>, IReportLayoutDao
{
    public ReportLayoutDao(IDataDomainScope dataDomainScope, IRepository<ReportLayout> repository) : base(dataDomainScope, repository)
    {
    }
}