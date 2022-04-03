using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Kapok.Report.Model;

public class Report
{
    [Required]
    public string? Name { get; set; }

    public Caption? Caption { get; set; }

    public IReportResourceProvider? Resources { get; set; }

    [XmlArray("Parameters")]
    [XmlArrayItem("Param")]
    public ReportParameterCollection Parameters { get; set; } = new();

    public Dictionary<string, IReportDataSet> DataSets { get; set; } = new();
}