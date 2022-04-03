using Kapok.Core;

namespace Kapok.View;

/// <summary>
/// A base class for a dialog page connected to one or multiple data sets.
/// </summary>
public abstract class DataDialogPage : DialogPage
{
    protected DataDialogPage(IViewDomain viewDomain, IDataDomain? dataDomain = null)
        : base(viewDomain)
    {
        DataDomainScope = dataDomain?.CreateScope() ?? DataDomain.Default.CreateScope();
    }

    protected DataDialogPage(IViewDomain viewDomain, IDataDomainScope? dataDomainScope = null)
        : base(viewDomain)
    {
        DataDomainScope = dataDomainScope ?? DataDomain.Default.CreateScope();
    }
        
    protected IDataDomainScope DataDomainScope { get; }
}