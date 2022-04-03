namespace Kapok.Report;

[Obsolete("Use IMimeTypeReportProcessor instead with mime type application/pdf")]
public interface IPdfReportProcessor : IReportProcessor
{
    void ProcessToPdfFile(string filePath);
}