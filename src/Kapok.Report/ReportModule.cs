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
        DataDomain.RegisterEntity<ReportDesign, ReportDesignService>();
        DataDomain.RegisterEntity<ReportDestination, ReportDestinationService>();
        DataDomain.RegisterEntity<ReportLayout, ReportLayoutService>();
        DataDomain.RegisterEntity<ReportModel, ReportModelService>();
        DataDomain.RegisterEntity<ReportProcessor, ReportProcessorService>();
    }
}