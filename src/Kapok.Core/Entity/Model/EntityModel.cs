using System.Reflection;

namespace Kapok.Entity.Model;

internal class EntityModel : IEntityModel
{
    public EntityModel(Type type)
    {
        Type = type;
    }

    public Type Type { get; }

    public PropertyInfo[]? PrimaryKeyProperties { get; set; }
    public List<IndexModel> Indexes { get; set; } = new();
    public List<EntityProperty> Properties { get; } =  new();
    public List<EntityRelationship> References { get; } = new();

    ICollection<IEntityProperty> IEntityModel.Properties => Properties.Cast<IEntityProperty>().ToList();
    ICollection<IIndexModel> IEntityModel.Indexes => Indexes.Cast<IIndexModel>().ToList();
    ICollection<IEntityRelationship> IEntityModel.References => References.Cast<IEntityRelationship>().ToList();
}