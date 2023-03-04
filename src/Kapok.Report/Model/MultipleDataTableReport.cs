using System.Dynamic;
using System.Xml.Serialization;

namespace Kapok.Report.Model;

public class DynamicReport : DynamicObject
{
    public string? Name { get; set; }

    public Caption? Caption { get; set; }

    [XmlArray("Parameters")]
    [XmlArrayItem("Param")]
    public List<ReportParameter>? Parameters { get; set; }
}
