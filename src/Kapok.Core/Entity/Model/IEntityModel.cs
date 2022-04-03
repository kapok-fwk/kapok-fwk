using System.Reflection;

namespace Kapok.Entity.Model;

public interface IEntityModel
{
    Type Type { get; }

    PropertyInfo[] PrimaryKeyProperties { get; }
    ICollection<IIndexModel> Indexes { get; }
    ICollection<IEntityProperty> Properties { get; }
    ICollection<IEntityRelationship> References { get; }
}