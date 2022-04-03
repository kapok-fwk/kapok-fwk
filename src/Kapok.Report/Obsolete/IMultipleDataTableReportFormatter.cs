using System.Data;
using System.Globalization;
using Kapok.Report.Model;
using OfficeOpenXml;

namespace Kapok.Report;

[Obsolete]
public interface IMultipleDataTableReportFormatter
{
    void FormatDataTableToExcelWorkbook(
        IReadOnlyList<Tuple<DataTableReport, DataTable, IDataTableReportFormatter>> reportResults,
        CultureInfo cultureInfo, ExcelWorkbook excelWorkbook);
}