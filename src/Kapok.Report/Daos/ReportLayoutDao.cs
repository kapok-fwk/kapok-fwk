using Kapok.Core;
using Kapok.Report.DataModel;

namespace Kapok.Report;

public class ReportLayoutDao : Dao<ReportLayout>, IReportLayoutDao
{
    public ReportLayoutDao(IDataDomainScope dataDomainScope) : base(dataDomainScope)
    {
    }
}