using System.Xml.Serialization;

namespace Kapok.Report.Model;

[Obsolete]
public class FixedValueList : DefaultValueList
{
    [XmlElement("Value")]
    public List<FixedListValue> Values { get; set; }

    public class FixedListValue
    {
        /// <summary>
        /// this is optional. If empty the DataValue is equal to DisplayValue
        /// </summary>
        [XmlAttribute]
        public string DataValue { get; set; }

        [XmlText] // TODO: Translation missing
        public string DisplayValue { get; set; }
    }
}