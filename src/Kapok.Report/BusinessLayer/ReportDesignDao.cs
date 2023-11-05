using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Report.DataModel;

namespace Kapok.Report.BusinessLayer;

public interface IReportDesignDao : IDao<ReportDesign>
{
}

public class ReportDesignDao : Dao<ReportDesign>, IReportDesignDao
{
    public ReportDesignDao(IDataDomainScope dataDomainScope, IRepository<ReportDesign> repository) : base(dataDomainScope, repository)
    {
    }
}