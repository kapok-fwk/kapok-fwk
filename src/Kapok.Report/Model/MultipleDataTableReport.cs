using System.Dynamic;
using System.Xml.Serialization;

namespace Kapok.Report.Model;

public class DynamicReport : DynamicObject
{
    // TODO [Required]
    public string Name { get; set; }

    public Caption Caption { get; set; }

    [XmlArray("Parameters")]
    [XmlArrayItem("Param")]
    public List<ReportParameter> Parameters { get; set; }
}

[ReportProcessor(typeof(MultipleDataTableReportProcessor))]
public class MultipleDataTableReport : Report
{
    public List<object> SubReports { get; set; }

    public Action<Report, dynamic> InitializeProcessorDelegate { get; set; }
}