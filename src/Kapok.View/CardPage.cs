using System.ComponentModel;
using Kapok.Core;

namespace Kapok.View;

/// <summary>
/// A base class for a card page.
/// </summary>
/// <typeparam name="TEntry"></typeparam>
public abstract class CardPage<TEntry> : DataPage<TEntry>, ICardPage, ICardPage<TEntry>
    where TEntry : class, new()
{
    protected CardPage(IDataSetView<TEntry> tableData, IViewDomain? viewDomain = null, IDataDomainScope? dataDomainScope = null)
        : base(tableData, viewDomain, dataDomainScope)
    {
        PropertyViewDefinitions = new PropertyViewCollection<TEntry>(ViewDomain, DataDomainScope.DataDomain, DataSet.GetDao().Model, DataSet);
        DataSet.PropertyChanged += DataSet_PropertyChanged;
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();
        PropertyViewDefinitions.RefreshPropertyLookups(false);
    }

    protected override void OnClosed()
    {
        DataSet.PropertyChanged -= DataSet_PropertyChanged;
        base.OnClosed();
    }
        
    private void DataSet_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Equals(e.PropertyName, nameof(IDataSetView<TEntry>.Current)))
            PropertyViewDefinitions.RefreshPropertyLookups(true);
    }

    /// <summary>
    /// Contains the properties which are shown in the view.
    /// </summary>
    public PropertyViewCollection<TEntry> PropertyViewDefinitions { get; }

    protected override void DeleteEntry(IList<TEntry> selectedEntries)
    {
        base.DeleteEntry(selectedEntries);
        Close();
    }

    #region ICardPage<TEntry>

    IList<PropertyView> ICardPage<TEntry>.PropertyViewDefinitions => this.PropertyViewDefinitions;

    #endregion
}