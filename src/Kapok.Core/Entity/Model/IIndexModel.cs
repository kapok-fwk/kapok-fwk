using System.Reflection;

namespace Kapok.Entity.Model;

public interface IIndexModel
{
    IReadOnlyList<PropertyInfo> Properties { get; }

    bool IsUnique { get; }
}