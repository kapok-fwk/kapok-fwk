using Kapok.Data;
using Kapok.Module;
using Kapok.Report.BusinessLayer;
using Kapok.Report.DataModel;

namespace Kapok.Report;

public sealed class ReportModule : ModuleBase
{
    public ReportModule() : base(nameof(ReportModel))
    {
    }

    public override void Initiate()
    {
        base.Initiate();

        // register DataModel
        DataDomain.RegisterEntity<ReportDesign, ReportDesignDao>();
        DataDomain.RegisterEntity<ReportDestination, ReportDestinationDao>();
        DataDomain.RegisterEntity<ReportLayout, ReportLayoutDao>();
        DataDomain.RegisterEntity<ReportModel, ReportModelDao>();
        DataDomain.RegisterEntity<ReportProcessor, ReportProcessorDao>();
    }
}