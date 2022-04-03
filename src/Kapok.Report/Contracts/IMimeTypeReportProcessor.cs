namespace Kapok.Report;

public interface IMimeTypeReportProcessor : IReportProcessor
{
    string[] SupportedMimeTypes { get; }
    void ProcessToStream(string mimeType, Stream stream);
}