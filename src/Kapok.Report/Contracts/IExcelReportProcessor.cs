using OfficeOpenXml;

namespace Kapok.Report;

public interface IExcelReportProcessor : IReportProcessor
{
    void ProcessToExcelStream(Stream stream);
}

public interface IExcelWorksheetReportProcessor : IReportProcessor
{
    void ProcessToExcelWorksheet(ExcelWorksheet excelWorksheet, int rowStart = 1, int colStart = 1);
}