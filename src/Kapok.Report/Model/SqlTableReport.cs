using System.Xml.Serialization;

namespace Kapok.Report.Model;

/// <summary>
/// A simple report which returns the result of an SQL query.
/// 
/// The class represents the data object which holds the definition
/// for the report (excluding the connection information for the database).
/// </summary>
public class SqlTableReport : Report
{
    public string SqlQuery { get; set; }

    [XmlArray("DataSourceParameters")]
    [XmlArrayItem("Param")]
    public List<string> DataSourceParameters { get; set; }

    public string DataSourceName { get; set; }
}

public class DataSetField
{
    public string SourceName { get; set; }

    public string Name { get; set; }

    public Caption Caption { get; set; }
}