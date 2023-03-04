using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Report.DataModel;

namespace Kapok.Report;

public class ReportLayoutDao : Dao<ReportLayout>, IReportLayoutDao
{
    public ReportLayoutDao(IDataDomainScope dataDomainScope) : base(dataDomainScope)
    {
    }
}