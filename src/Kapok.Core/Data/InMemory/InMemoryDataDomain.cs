namespace Kapok.Data.InMemory;

public class InMemoryDataDomain : DataDomain
{
    public override IDataDomainScope CreateScope()
    {
        return new InMemoryDataDomainScope(this);
    }
}