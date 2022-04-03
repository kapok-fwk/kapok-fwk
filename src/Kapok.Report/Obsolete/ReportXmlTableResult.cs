using System.Xml.Serialization;

namespace Kapok.Report.Xml;

[Obsolete]
public class ReportXmlTableResult
{
    public HeadDefinition Head { get; set; }

    public BodyDefinition Body { get; set; }

    public class HeadDefinition
    {
        public string Name { get; set; }

        [XmlElement("Meta")]
        public List<MetaDefinition> MetaLines { get; set; }

        public class MetaDefinition
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlText]
            public string Value { get; set; }
        }
    }

    public class BodyDefinition
    {
        [XmlElement("Table")]
        public List<TableDefinition> Tables { get; set; }

        public class TableDefinition
        {
            [XmlArray("Columns")]
            [XmlArrayItem("Column")]
            public List<ColumnDefinition> Columns { get; set; }

            [XmlArray("Rows")]
            [XmlArrayItem("Row")]
            public List<RowDefinition> Rows { get; set; }

            public class ColumnDefinition
            {
                [XmlAttribute]
                public string Name { get; set; }

                [XmlText]
                public string Caption { get; set; }
            }

            public class RowDefinition
            {
                [XmlElement("ByteCell", typeof(byte))]
                [XmlElement("ShortCell", typeof(short))]
                [XmlElement("IntCell", typeof(int))]
                [XmlElement("FloatCell", typeof(float))]
                [XmlElement("DoubleCell", typeof(double))]
                [XmlElement("DecimalCell", typeof(decimal))]
                [XmlElement("CharCell", typeof(char))]
                [XmlElement("BoolCell", typeof(bool))]
                [XmlElement("StringCell", typeof(string))]
                [XmlElement("DateTimeCell", typeof(DateTime))]
                [XmlElement("GuidCell", typeof(Guid))]
                public List<object> Value { get; set; }
            }
        }
    }
}