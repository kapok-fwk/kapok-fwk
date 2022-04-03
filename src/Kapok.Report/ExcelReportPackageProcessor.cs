namespace Kapok.Report;

public class ExcelReportPackageProcessor : ReportProcessor<ExcelReportPackage>
{
    #region Static members

    static ExcelReportPackageProcessor()
    {
        ReportEngine.RegisterProcessor(typeof(ExcelReportPackageProcessor), typeof(ExcelReportPackage));
    }

    public static void Register()
    {
        // this function can be called to make sure that the static constructor is called.
    }

    #endregion

    private readonly ReportEngine _reportEngine;

    public ExcelReportPackageProcessor(ReportEngine reportEngine)
    {
        _reportEngine = reportEngine;
    }

    public const string XlsxMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public override string[] SupportedMimeTypes => new[] { XlsxMimeType };

    public override void ValidateRequiredFields()
    {
        // TODO: currently 'ParameterValues' is required. This property is to be deprecated in the future
        ParameterValues ??= new();

        base.ValidateRequiredFields();
    }

    public override void ProcessToStream(string mimeType, Stream stream)
    {
        ValidateRequiredFields();
        ValidateReportModel();

        switch (mimeType)
        {
            case XlsxMimeType:
#pragma warning disable CS8602
                using (var package = ReportModel.Build(_reportEngine))
#pragma warning restore CS8602
                {
                    package.SaveAs(stream);
                }
                break;
            default:
                throw new NotSupportedException($"Not supported mime type: {mimeType}");
        }
    }
}