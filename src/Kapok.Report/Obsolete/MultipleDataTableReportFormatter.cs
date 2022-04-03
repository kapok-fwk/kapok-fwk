using System.Data;
using System.Globalization;
using Kapok.Report.Model;
using OfficeOpenXml;

namespace Kapok.Report;

[Obsolete]
public class MultipleDataTableReportFormatter : IMultipleDataTableReportFormatter
{
    private static MultipleDataTableReportFormatter _defaultFormatter;

    public static MultipleDataTableReportFormatter Default => _defaultFormatter ??= new MultipleDataTableReportFormatter();

    public virtual void FormatDataTableToExcelWorkbook(IReadOnlyList<Tuple<DataTableReport, DataTable, IDataTableReportFormatter>> reportResults, CultureInfo cultureInfo, ExcelWorkbook excelWorkbook)
    {
        foreach (var reportResult in reportResults)
        {
            var ws = excelWorkbook.Worksheets.Add(reportResult.Item1.Caption.LanguageOrDefault(cultureInfo) ?? reportResult.Item1.Name);

            (reportResult.Item3 ?? DataTableReportFormatter.Default).FormatDataTableToExcelWorksheet(
                dataTable: reportResult.Item2,
                report: reportResult.Item1,
                cultureInfo: cultureInfo,
                excelWorksheet: ws);
        }
    }
}