using System.Reflection;

namespace Kapok.Entity.Model;

internal class IndexModel : IIndexModel
{
    public IndexModel(PropertyInfo[] properties)
    {
        Properties = properties;
    }

    public PropertyInfo[] Properties { get; set; }
    public bool IsUnique { get; set; }

    IReadOnlyList<PropertyInfo> IIndexModel.Properties => Properties;
}