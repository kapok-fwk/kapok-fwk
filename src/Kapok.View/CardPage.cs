using System.ComponentModel;
using Kapok.Entity;

namespace Kapok.View;

/// <summary>
/// A base class for a card page.
/// </summary>
/// <typeparam name="TEntry"></typeparam>
public abstract class CardPage<TEntry> : DataPage<TEntry>, ICardPage, ICardPage<TEntry>
    where TEntry : class, new()
{
    private PropertyViewCollection<TEntry>? _propertyViewDefinitions;

    protected CardPage(IServiceProvider serviceProvider, IDataSetView<TEntry> tableData)
        : base(serviceProvider, tableData)
    {
    }

    protected override void OnLoaded()
    {
#pragma warning disable CS8602
        DataSet.PropertyChanged += DataSet_PropertyChanged;
#pragma warning restore CS8602
        base.OnLoaded();
        PropertyViewDefinitions.RefreshPropertyLookups(false);
    }

    protected override void OnClosed()
    {
        if (DataSet != null)
        {
            DataSet.PropertyChanged -= DataSet_PropertyChanged;
        }
        base.OnClosed();
    }
        
    private void DataSet_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDataSetView<TEntry>.Current))
            PropertyViewDefinitions.RefreshPropertyLookups(true);
    }

    /// <summary>
    /// Contains the properties which are shown in the view.
    /// </summary>
    public PropertyViewCollection<TEntry> PropertyViewDefinitions =>
        _propertyViewDefinitions ??= new PropertyViewCollection<TEntry>(ServiceProvider,
            EntityBase.GetEntityModel<TEntry>(), DataSet);

    protected override void DeleteEntry(IList<TEntry?>? selectedEntries)
    {
        base.DeleteEntry(selectedEntries);
        Close();
    }

    #region ICardPage<TEntry>

    IList<PropertyView> ICardPage<TEntry>.PropertyViewDefinitions => PropertyViewDefinitions;

    #endregion
}