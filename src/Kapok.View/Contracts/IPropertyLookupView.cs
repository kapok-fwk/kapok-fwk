using Kapok.Entity;

namespace Kapok.View;

public interface IPropertyLookupView
{
    ILookupDefinition LookupDefinition { get; }

    void Refresh();
}