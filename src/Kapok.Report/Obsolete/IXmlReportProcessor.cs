using System.Xml;

namespace Kapok.Report;

[Obsolete("Use IMimeTypeReportProcessor instead with mime type text/xml")]
public interface IXmlReportProcessor : IReportProcessor
{
    void ProcessToXml(Stream stream);
    void ProcessToXml(TextWriter textWriter);
    void ProcessToXml(XmlWriter xmlWriter);
}