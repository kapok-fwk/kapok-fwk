using Kapok.Core;

namespace Kapok.View;

/// <summary>
/// A base class for a detail page.
/// 
/// A detail page can be added to a class inheriting the class InteractivePage.
/// </summary>
/// <typeparam name="TEntry"></typeparam>
public abstract class DetailPage<TEntry> : DataPage<TEntry>, IDetailPage<TEntry>
    where TEntry : class, new()
{
    protected DetailPage(IDataSetView<TEntry> tableData, IViewDomain viewDomain, IDataDomainScope? dataDomainScope = null)
        : base(tableData, viewDomain, dataDomainScope)
    {
        IsClosed = false;
    }

    internal DetailPage(IViewDomain? viewDomain = null, IDataDomainScope? dataDomainScope = null)
        : base(viewDomain, dataDomainScope)
    {
        IsClosed = false;
    }

    private bool _isClosed;

    protected override void OnLoaded()
    {
        Logger.Info("Load detail window {window_class}", GetType().Name);
        base.OnLoaded();
    }

    // TODO: this override is a bit dirty; is required because we did not register an window object to the DetailPage in the WpfViewModel
    protected override void EndEdit()
    {
    }

    public override void Close()
    {
        base.Close();
        IsClosed = true;
    }

    public bool IsClosed
    {
        get => _isClosed;
        set => SetProperty(ref _isClosed, value);
    }
}