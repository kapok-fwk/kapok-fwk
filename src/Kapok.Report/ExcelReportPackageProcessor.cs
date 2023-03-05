namespace Kapok.Report;

public class ExcelReportPackageProcessor : ReportProcessor<ExcelReportPackage>
{
    #region Static members

    static ExcelReportPackageProcessor()
    {
        ReportEngine.RegisterProcessor(typeof(ExcelReportPackageProcessor), typeof(ExcelReportPackage));
    }

    /// <summary>
    /// This method can be called to make sure that the static constructor is called.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static void Register()
    {
    }

    #endregion

    private readonly ReportEngine _reportEngine;

    public ExcelReportPackageProcessor(ReportEngine reportEngine)
    {
        _reportEngine = reportEngine;
    }

    public const string XlsxMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public override string[] SupportedMimeTypes => new[] { XlsxMimeType };

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