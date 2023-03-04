using System.Xml.Serialization;

namespace Kapok.Report.Model;

public class HtmlReport : Report
{
    /// <summary>
    /// The HTML template to be used. Can be defined individually per culture.
    /// </summary>
    [XmlIgnore]
    public Caption? Template { get; set; }

    /// <summary>
    /// As an alternative for <see cref="Template"/>, a resource name for the HTML template; per culture.
    /// </summary>
    public Caption? TemplateResourceName { get; set; }
}