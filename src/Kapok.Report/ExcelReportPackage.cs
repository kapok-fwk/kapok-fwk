using OfficeOpenXml;

namespace Kapok.Report;

/// <summary>
/// A report for building a excel report package;  meaning:
/// A excel report containing several other excel reports (= worksheets)
/// </summary>
public class ExcelReportPackage : ExcelReport
{
    /// <summary>
    /// A list of excel reports which shall be added to the excel report package.
    /// </summary>
    public List<ExcelReport> SubReports { get; set; } = new();

    public virtual ExcelPackage Build(ReportEngine reportEngine)
    {
        var package = new ExcelPackage();

        foreach (var subReport in SubReports)
        {
            // pass resource provider to the sub report
            subReport.Resources = Resources;

            // pass report parameters to the sub report
            foreach (var reportParameter in Parameters)
            {
                if (subReport.Parameters.Contains(reportParameter.Name))
                    subReport.Parameters[reportParameter.Name].Value = reportParameter.Value;
            }

            // build the sub report
            subReport.Build(reportEngine, package.Workbook);
        }

        return package;
    }

    public override ExcelWorksheet Build(ReportEngine reportEngine, ExcelWorkbook workbook)
    {
        throw new NotSupportedException($"This method is not supported from type {nameof(ExcelReportPackage)}.");
    }
}