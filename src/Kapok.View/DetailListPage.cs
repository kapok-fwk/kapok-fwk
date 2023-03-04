using Kapok.Data;

namespace Kapok.View;

/// <summary>
/// A base class for a detail page showing a list.
/// </summary>
/// <typeparam name="TBaseEntry"></typeparam>
/// <typeparam name="TLinkedEntry"></typeparam>
public abstract class DetailListPage<TBaseEntry, TLinkedEntry> : LinkedDetailPage<TBaseEntry, TLinkedEntry>, IDetailListPage<TBaseEntry, TLinkedEntry>
    where TBaseEntry : class, new()
    where TLinkedEntry : class, new()
{
    protected DetailListPage(IDataSetView<TBaseEntry> sourceDataSet, IViewDomain? viewDomain = null, IDataDomainScope? dataDomainScope = null)
        : base(sourceDataSet, viewDomain, dataDomainScope)
    {
    }

    protected override IDataSetView<TLinkedEntry> InitializeBaseDataSet()
    {
        return ViewDomain.CreateDataSetView<TLinkedEntry>(DataDomainScope);
    }

    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable MemberCanBeProtected.Global
    public IDataSetSelectionAction<TLinkedEntry>? OpenCardPageAction { get; protected set; }
    // ReSharper restore MemberCanBeProtected.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore InconsistentNaming

    protected override bool CanCreateNewEntry()
    {
        return SourceDataSet.Current != null && base.CanCreateNewEntry();
    }
}