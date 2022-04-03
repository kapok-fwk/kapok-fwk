namespace Kapok.Core;

/// <summary>
/// An business layer object that handles data
/// from the global repository.
/// </summary>
public abstract class DbBusinessLayerBase : BusinessLayerBase
{
    protected DbBusinessLayerBase(IDataDomainScope dataDomainScope)
    {
        DataDomainScope = dataDomainScope;
    }

    protected readonly IDataDomainScope DataDomainScope;
}