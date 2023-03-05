using OfficeOpenXml;

namespace Kapok.Report;

public interface IExcelWorksheetReportProcessor : IReportProcessor
{
    void ProcessToExcelWorksheet(ExcelWorksheet excelWorksheet, int rowStart = 1, int colStart = 1);
}