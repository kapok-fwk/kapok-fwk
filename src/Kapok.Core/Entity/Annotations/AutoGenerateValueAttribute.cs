namespace Kapok.Entity;

public enum AutoGenerateValueType
{
    /// <summary>
    /// Set to the record the date/time when the record was created.
    /// 
    /// Changes to this value in the business layer will be ignored.
    ///
    /// Can only be applied to properties of the type DateTime.
    /// </summary>
    CreatedDateTime,

    /// <summary>
    /// Set to the record the date/time when the record was last modified.
    /// The value will be set when an creation/modification of the record takes
    /// place and it is saved via the DbContext.
    ///
    /// Changes to this value in the business layer will be overriden.
    ///
    /// Can only be applied to properties of the type DateTime.
    /// </summary>
    LastModifiedDateTime
}

public class AutoGenerateValueAttribute : Attribute
{
    public AutoGenerateValueAttribute(AutoGenerateValueType type)
    {
        Type = type;
    }

    public AutoGenerateValueType Type { get; }
}