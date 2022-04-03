using Kapok.Core;
using Kapok.Report.DataModel;

namespace Kapok.Report;

public interface IReportModelDao : IDao<ReportModel>
{
    ReportModel GetOrCreateFromType(Type reportModelType);
    Type GetType(ReportModel entity);
}