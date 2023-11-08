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
    protected DetailPage(IServiceProvider serviceProvider, IDataSetView<TEntry> tableData)
        : base(serviceProvider, tableData)
    {
    }

    internal DetailPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    // ReSharper disable once RedundantDefaultMemberInitializer
    private bool _isClosed = false;

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