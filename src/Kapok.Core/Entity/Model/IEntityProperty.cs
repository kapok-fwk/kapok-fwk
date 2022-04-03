namespace Kapok.Entity.Model;

public interface IEntityProperty
{
    string PropertyName { get; }

    IPropertyCalculateDefinition? CalculateDefinition { get; }

    ILookupDefinition? LookupDefinition { get; }

    IDrillDownDefinition? DrillDownDefinition { get; }
}