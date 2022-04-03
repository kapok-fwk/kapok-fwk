namespace Kapok.Entity;

/// <summary>
/// Defines the precision of a float pointing type.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PrecisionAttribute : Attribute
{
    public PrecisionAttribute(int precision, int scale)
    {
        if (precision < 0) throw new ArgumentException("Precision can not be a negative number", nameof(precision));
        if (scale < 0) throw new ArgumentException("Scale can not be a negative number", nameof(scale));

        Precision = precision;
        Scale = scale;
    }

    public PrecisionAttribute(int precision)
    {
        if (precision < 0) throw new ArgumentException("Precision can not be a negative number", nameof(precision));

        Precision = precision;
    }

    /// <summary>
    /// The precision of the property.
    /// </summary>
    public int Precision { get; }

    /// <summary>
    /// The scale of the property.
    /// </summary>
    public int? Scale { get; }
}