namespace Kapok.Entity;

[AttributeUsage(AttributeTargets.Property)]
public class LookupColumnAttribute : Attribute
{
    public LookupColumnAttribute()
    {
        Show = true;
    }

    public LookupColumnAttribute(bool show)
    {
        Show = show;
    }

    public bool Show { get; set; }
}