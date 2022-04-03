namespace Kapok.Report;

public interface IHtmlReportProcessor : IReportProcessor
{
    string ProcessToHtml();
}