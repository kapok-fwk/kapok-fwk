using Kapok.Core;
using Kapok.Report.DataModel;

namespace Kapok.Report;

public interface IReportProcessorDao : IDao<ReportProcessor>
{
    Task<ReportProcessor> GetOrCreateFromType(Type reportProcessorType);
    Type GetType(ReportProcessor entity);
}