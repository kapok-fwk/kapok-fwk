namespace Kapok.Entity.Model;

internal class EntityProperty : IEntityProperty
{
    public EntityProperty(string propertyName)
    {
        PropertyName = propertyName;
    }

    public string PropertyName { get; set; }
    public IPropertyCalculateDefinition? CalculateDefinition { get; set; }
    public ILookupDefinition? LookupDefinition { get; set; }
    public IDrillDownDefinition? DrillDownDefinition { get; set; }
}