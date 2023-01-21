using System.Xml.Serialization;

namespace Kapok.Report.Model;

public abstract class DataTableReport : Report
{
    [XmlArray("Fields")]
    [XmlArrayItem("Field")]
    public List<DataSetField>? Fields { get; set; }
}