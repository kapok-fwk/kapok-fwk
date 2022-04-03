namespace Kapok.View;

[AttributeUsage(AttributeTargets.Property)]
public class MenuItemAttribute : Attribute
{
    public string MenuName { get; set; }

    // TODO: we need a translation for this field
    public string TabName { get; set; }

    public string KeyTip { get; set; }

    public int Order { get; set; }
}