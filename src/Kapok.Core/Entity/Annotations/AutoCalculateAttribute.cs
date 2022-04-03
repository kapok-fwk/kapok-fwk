using System.ComponentModel.DataAnnotations.Schema;

namespace Kapok.Entity;

/// <summary>
/// Indicates that the field is an auto-calculated field. The field will not be stored in the database.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AutoCalculateAttribute : NotMappedAttribute
{
    public AutoCalculateAttribute()
    {
    }

    public AutoCalculateAttribute(string methodName)
    {
        MethodName = methodName;
    }

    public string? MethodName { get; }
}