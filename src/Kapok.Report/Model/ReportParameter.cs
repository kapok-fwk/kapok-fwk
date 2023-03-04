using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Xml.Serialization;

namespace Kapok.Report.Model;


public class ReportParameter
{
#pragma warning disable CS8618
    public ReportParameter(string name, Type dataType)
#pragma warning restore CS8618
    {
        Name = name;
        DataType = dataType;
    }

    /// <summary>
    /// The internal name of the report parameter.
    ///
    /// This should not contain whitespace characters and - if possible -
    /// only use ASCII characters to e.g. be able to use them as e.g.
    /// SQL command parameter.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The visible UI name for the report parameter.
    /// </summary>
    public Caption? Caption { get; set; }

    private Type _dataType;
    private string? _xmlWrapperDataType;

    [XmlIgnore]
    public Type DataType
    {
        get => _dataType;
        set
        {
            _dataType = value;
            _xmlWrapperDataType = value.FullName;
        }
    }

    [XmlElement("DataType")]
    public string? XmlWrapperDataType
    {
        get => _xmlWrapperDataType;
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _xmlWrapperDataType = value;
#pragma warning disable CS8601
            _dataType = Type.GetType(value);
#pragma warning restore CS8601
        }
    }

    public object? DefaultValue { get; set; }

    private bool _valueIsSet;
    private object? _value;

    /// <summary>
    /// The current value set to the parameter.
    /// 
    /// If not set, the DefaultValue will be given.
    /// </summary>
    [NotMapped, XmlIgnore]
    public object? Value
    {
        get
        {
            if (_valueIsSet)
                return _value;
            return DefaultValue;
        }
        set
        {
            _value = value;
            _valueIsSet = true;
        }
    }

    public IList<object>? DefaultIterativeValues { get; set; }
        
    /// <summary>
    /// All empty values are passed to the SQl Query as an DbNull Value.
    /// </summary>
    public bool HandleEmptyValueAsNull { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("ReportParameter: ");
        sb.Append("(");
        sb.Append(DataType.FullName);
        sb.Append(") ");

        sb.Append(Name);
        if (Value != null)
        {
            sb.Append(" = ");
            sb.Append(Value);
        }

        return sb.ToString();
    }
}