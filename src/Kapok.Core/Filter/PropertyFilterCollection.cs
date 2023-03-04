using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Kapok.BusinessLayer;

public class PropertyFilterCollection<T> : FilterBase<T>, IDisposable, IPropertyFilterCollection<T>
{
    public PropertyFilterCollection()
    {
        Properties = new ObservableCollection<IPropertyFilter>();
        Properties.CollectionChanged += Properties_CollectionChanged;
    }
        
    // TODO: we should replace dispose with something else, like an Unload event ...
    public void Dispose()
    {
        Properties.Clear();
        Properties.CollectionChanged -= Properties_CollectionChanged;
    }

    private Expression<Func<T, bool>>? _filterExpressionCache;

    private void Properties_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        bool filterChanged = false;
        bool checkNewItem = false;
        bool checkOldItem = false;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                checkNewItem = true;
                break;
            case NotifyCollectionChangedAction.Remove:
                checkOldItem = true;
                break;
            case NotifyCollectionChangedAction.Replace:
                checkOldItem = true;
                checkNewItem = true;
                break;
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Reset:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
            
        if (checkOldItem && e.OldItems != null)
        {
            foreach (var oldItem in e.OldItems.Cast<IPropertyFilter>())
            {
                if (oldItem is PropertyFilter propertyFilter)
                {
                    if (!((INotifyDataErrorInfo)propertyFilter).HasErrors)
                        filterChanged = true;
                }
                else
                {
                    filterChanged = true;
                }

                oldItem.FilterChanged -= IPropertyFilter_FilterChanged;
            }
        }

        if (checkNewItem && e.NewItems != null)
        {
            foreach (var newItem in e.NewItems.Cast<IPropertyFilter>())
            {
                if (newItem is PropertyFilter propertyFilter)
                {
                    if (!((INotifyDataErrorInfo)propertyFilter).HasErrors)
                        filterChanged = true;
                }
                else
                {
                    filterChanged = true;
                }

                newItem.FilterChanged += IPropertyFilter_FilterChanged;
            }
        }

        if (!filterChanged) return;

        OnFilterChanged();
    }

    private void IPropertyFilter_FilterChanged(object? sender, EventArgs e)
    {
        OnFilterChanged();
    }

    // NOTE: This property is on purpose not implemented as ObservableCollection<IPropertyFilter<T>> because this would lead to troubles when we want to access this ObservableCollection at a place where we don't know the generic type T (e.g. in the GUI control DataGrid with the header filter extension)
    public ObservableCollection<IPropertyFilter> Properties { get; }

    private Expression<Func<T, bool>>? CreateFilterExpression()
    {
        Expression<Func<T, bool>>? lastExpression = null;

        foreach (var propertyFilter in Properties)
        {
            var expression = (Expression<Func<T, bool>>?)propertyFilter.FilterExpression;
            if (expression == null)
                continue;

            if (lastExpression == null)
                lastExpression = expression;
            else
                lastExpression = lastExpression.AndAlso(expression);
        }

        return lastExpression;
    }

    public void ReplacePropertyFilter(IPropertyFilter<T> oldFilter, IPropertyFilter<T> newFilter)
    {
        lock (Properties)
        {
            var index = Properties.IndexOf(oldFilter);

            if (index < 0)
                throw new NotSupportedException("The old filter does not exist in the filter collection.");

            Properties[index] = newFilter;
        }
    }

    public override void Clear()
    {
        _skipFilterChangedNotification = true;

        // this is not done in Properties_CollectionChanged because the items are not given in e.OldItems,
        // so we have to perform this here to avoid binding issues (= memory leak because GC cannot free the items)
        foreach (var propertyFilter in Properties)
        {
            propertyFilter.FilterChanged -= IPropertyFilter_FilterChanged;
        }

        Properties.Clear();
        _skipFilterChangedNotification = false;
        OnFilterChanged();
    }

    public override Expression<Func<T, bool>>? FilterExpression => _filterExpressionCache ??= CreateFilterExpression();

    private bool _skipFilterChangedNotification;

    protected override void OnFilterChanged()
    {
        if (_skipFilterChangedNotification)
            return;
        _filterExpressionCache = null;
        base.OnFilterChanged();
    }

    #region IPropertyFilterCollection
    ICollection<IPropertyFilter> IPropertyFilterCollection.Properties => Properties;

    void IPropertyFilterCollection.ReplacePropertyFilter(IPropertyFilter oldFilter, IPropertyFilter newFilter)
    {
        ReplacePropertyFilter((IPropertyFilter<T>)oldFilter, (IPropertyFilter<T>)newFilter);
    }

    #endregion
}