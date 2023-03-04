using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Report.DataModel;

namespace Kapok.Report;

public class ReportDesignDao : Dao<ReportDesign>, IReportDesignDao
{
    public ReportDesignDao(IDataDomainScope dataDomainScope) : base(dataDomainScope)
    {
    }
}