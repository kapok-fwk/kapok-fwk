using Kapok.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Kapok.View;

/// <summary>
/// A base class for a dialog page connected to one or multiple data sets.
/// </summary>
public abstract class DataDialogPage : DialogPage
{
    private IDataDomainScope? _dataDomainScope;

    protected DataDialogPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected IDataDomain DataDomain => ServiceProvider.GetRequiredService<IDataDomain>();

    protected IDataDomainScope DataDomainScope => _dataDomainScope ??= ServiceProvider.GetService<IDataDomainScope>() ?? DataDomain.CreateScope();
}