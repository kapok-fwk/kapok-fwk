namespace Kapok.View;

/// <summary>
/// A base class for a detail page which shows data referenced to a source data set (e.g. the main data set from the main page).
/// </summary>
/// <typeparam name="TBaseEntry"></typeparam>
/// <typeparam name="TLinkedEntry"></typeparam>
public abstract class LinkedDetailPage<TBaseEntry, TLinkedEntry> : DetailPage<TLinkedEntry>, ILinkedDetailPage<TBaseEntry, TLinkedEntry>
    where TBaseEntry : class, new()
    where TLinkedEntry : class, new()
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once StaticMemberInGenericType
    public static string SourceDataSetName = "Source";

    protected LinkedDetailPage(IServiceProvider serviceProvider, IDataSetView<TBaseEntry> sourceDataSet)
        : base(serviceProvider)
    {
        SourceDataSet = sourceDataSet;
        SourceDataSet.PropertyChanged += SourceDataSet_PropertyChanged;
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();
        LinkChanged(SourceDataSet.Current);
    }

    protected override void OnClosed()
    {
        SourceDataSet.PropertyChanged -= SourceDataSet_PropertyChanged;
        base.OnClosed();
    }

    private void SourceDataSet_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // NOTE: we currently support with this class only single selections as link, not multi selections

        if (e.PropertyName == nameof(IDataSetView.Current))
            LinkChanged(SourceDataSet.Current);
    }

    public IDataSetView<TBaseEntry> SourceDataSet { get; }

    /// <summary>
    /// Is called when the link between the other DataSet is changed (e.g. the current selected entry has changed).
    /// </summary>
    protected abstract void LinkChanged(TBaseEntry? baseEntry);
}