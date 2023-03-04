using Kapok.Entity.Model;

namespace Kapok.View;

public interface IPropertyLookupView
{
    ILookupDefinition LookupDefinition { get; }

    void Refresh();
}