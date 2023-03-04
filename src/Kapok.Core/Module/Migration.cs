using Kapok.Data;

namespace Kapok.Module;

/// <summary>
/// A migration script
/// </summary>
public abstract class Migration
{
    // this is implemented internal because we don't want to give access direct user access to the up/down migration calls
    internal void ExecuteUp(IDataDomainScope scope)
    {
        Up(scope);
    }

    protected abstract void Up(IDataDomainScope scope);

    protected virtual void Down(IDataDomainScope scope)
    {
    }
}