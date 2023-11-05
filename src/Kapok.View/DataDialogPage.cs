using Kapok.Data;

namespace Kapok.View;

/// <summary>
/// A base class for a dialog page connected to one or multiple data sets.
/// </summary>
public abstract class DataDialogPage : DialogPage
{
    private IDataDomainScope? _dataDomainScope;

    protected DataDialogPage(IViewDomain? viewDomain = null, IDataDomain? dataDomain = null)
        : base(viewDomain)
    {
        DataDomain = dataDomain
                     ?? Data.DataDomain.Default
                     ?? throw new NotSupportedException(
                         $"You have to first set Kapok.Core.DataDomain.Default before you can initiate a page without {nameof(dataDomain)} being provided");
    }

    protected DataDialogPage(IViewDomain? viewDomain = null, IDataDomainScope? dataDomainScope = null)
        : this(viewDomain, dataDomainScope?.DataDomain)
    {
        _dataDomainScope = dataDomainScope;
    }

    protected IDataDomain DataDomain { get; }

    protected IDataDomainScope DataDomainScope => _dataDomainScope ??= DataDomain.CreateScope();

    protected override void OnClosed()
    {
        base.OnClosed();

        DataDomainScope.UnregisterUsage(this);
        _dataDomainScope = null;
    }
}