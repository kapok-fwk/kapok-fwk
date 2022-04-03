namespace Kapok.Report.Model;

public abstract class PrintableReport : Report
{
    public PrintableReportType ReportType { get; set; } = PrintableReportType.List;
}

public enum PrintableReportType
{
    List,
    Card,
    Label
}