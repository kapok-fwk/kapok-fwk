using Kapok.Data;

namespace Kapok.View;

/// <summary>
/// A base class for a dialog page connected to one or multiple data sets.
/// </summary>
public abstract class DataDialogPage : DialogPage
{
    protected DataDialogPage(IViewDomain? viewDomain = null, IDataDomain? dataDomain = null)
        : base(viewDomain)
    {
        if (dataDomain == null && DataDomain.Default == null)
            throw new NotSupportedException(
                $"You have to first set Kapok.Core.DataDomain.Default before you can initiate a page without {nameof(dataDomain)} being provided");
#pragma warning disable CS8602
        DataDomainScope = (dataDomain ?? DataDomain.Default).CreateScope();
#pragma warning restore CS8602
    }

    protected DataDialogPage(IViewDomain? viewDomain = null, IDataDomainScope? dataDomainScope = null)
        : base(viewDomain)
    {
        if (dataDomainScope == null && DataDomain.Default == null)
            throw new NotSupportedException(
                $"You have to first set Kapok.Core.DataDomain.Default before you can initiate a page without {nameof(dataDomainScope)} being provided");
#pragma warning disable CS8602
        DataDomainScope = dataDomainScope ?? DataDomain.Default.CreateScope();
#pragma warning restore CS8602
    }
        
    protected IDataDomainScope DataDomainScope { get; }
}