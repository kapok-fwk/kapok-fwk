using System.Data;
using System.Globalization;
using Kapok.Report.Model;
using OfficeOpenXml;

namespace Kapok.Report;

[Obsolete]
public interface IDataTableReportFormatter
{
    void FormatDataTable(DataTable dataTable, DataTableReport reportModel, CultureInfo cultureInfo);

    void FormatDataTableToCsv(DataTable dataTable, DataTableReport report, CultureInfo cultureInfo,
        Stream csvStream);

    void FormatDataTableToExcelWorksheet(DataTable dataTable, DataTableReport report,
        CultureInfo cultureInfo, ExcelWorksheet excelWorksheet, int rowStart = 1, int colStart = 1);
}