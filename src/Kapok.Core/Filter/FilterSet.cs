using System.Linq.Expressions;

namespace Kapok.BusinessLayer;

public class FilterSet<T> : FilterBase<T>, IDisposable, IFilterSet<T>
    where T : class
{
    private readonly Dictionary<FilterLayer, IFilter<T>> _layers = new();

    public IReadOnlyDictionary<FilterLayer, IFilter<T>> Layers => _layers;

    /// <summary>
    /// Returns the user layer filter which is always of the type <code>PropertyFilter&lt;T&gt;</code>.
    /// </summary>
    public PropertyFilterCollection<T> UserLayer
    {
        get
        {
            if (!Layers.ContainsKey(FilterLayer.User))
                Add(new PropertyFilterCollection<T>());
                
            return (PropertyFilterCollection<T>)Layers[FilterLayer.User];
        }
    }

    public void Dispose()
    {
        foreach (var filter in _layers.Values)
        {
            filter.FilterChanged -= LayerFilter_FilterChanged;
        }
    }
        
    private void LayerFilter_FilterChanged(object? sender, EventArgs e)
    {
        OnFilterChanged();
    }

    public void Add(IFilterSet<T> filterSet)
    {
        foreach (var filterLayerPair in filterSet.Layers)
        {
            if (!Layers.ContainsKey(filterLayerPair.Key))
            {
                Add(filterLayerPair.Value, filterLayerPair.Key);
                continue;
            }

            bool filterIsSet = false;
            if (filterLayerPair.Value is IPropertyFilterCollection newPropertyFilterCollection)
            {
                IFilter currentFilter = Layers[filterLayerPair.Key];

                if (currentFilter is IPropertyFilterCollection currentPropertyFilterCollection)
                {
                    foreach (var propertyFilter in newPropertyFilterCollection.Properties)
                    {
                        currentPropertyFilterCollection.Properties.Add(propertyFilter);
                    }

                    filterIsSet = true;
                }
            }

            if (!filterIsSet)
            {
                Add(filterLayerPair.Value, filterLayerPair.Key);
            }
        }
    }

    /// <summary>
    /// Adds a new filter to the filter set.
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="filter"></param>
    public void Add(IFilter<T> filter, FilterLayer layer = FilterLayer.User)
    {
        if (filter is FilterSet<T>)
            throw new NotSupportedException($"A FilterSet<> can not be added to an FilterSet<> object.");

        if (layer == FilterLayer.User && !(filter is IPropertyFilterCollection || filter is IPropertyFilter))
            throw new NotSupportedException(
                $"A filter for the layer {FilterLayer.User} must implement the interface {typeof(IPropertyFilterCollection).FullName}.");

        if (!_layers.ContainsKey(layer))
        {
            IFilter<T> filterToAdd;
            if (filter is IPropertyFilter<T> propertyFilter1)
            {
                var newPropertyFilterCollection = new PropertyFilterCollection<T>();
                newPropertyFilterCollection.Properties.Add(propertyFilter1);
                filterToAdd = newPropertyFilterCollection;
            }
            else
            {
                filterToAdd = filter;
            }

            filterToAdd.FilterChanged += LayerFilter_FilterChanged;
            _layers.Add(layer, filterToAdd);

            if (filterToAdd.FilterExpression != null)
                OnFilterChanged();
            return;
        }

        if (_layers[layer] is IPropertyFilterCollection propertyFilterCollection &&
            filter is IPropertyFilter<T> propertyFilter2)
        {
            propertyFilterCollection.Properties.Add(propertyFilter2);
            return;
        }

        Expression<Func<T, bool>>? currentFilterExpression = _layers[layer].FilterExpression;
        if (currentFilterExpression == null)
        {
            // replace
            _layers[layer].FilterChanged -= LayerFilter_FilterChanged;
            filter.FilterChanged += LayerFilter_FilterChanged;
            _layers[layer] = filter;

            if (filter.FilterExpression != null)
                OnFilterChanged();
            return;
        }

        var filterExpressionToAdd = filter.FilterExpression;
        if (filterExpressionToAdd == null)
            return;

        var newFilterExpression = currentFilterExpression.AndAlso(filterExpressionToAdd);

        if (_layers[layer] is Filter<T> currentFilterChangeable)
        {
            _layers[layer].FilterChanged -= LayerFilter_FilterChanged;
            currentFilterChangeable.FilterExpression = newFilterExpression;
            _layers[layer].FilterChanged += LayerFilter_FilterChanged;
        }
        else if (filter is Filter<T> newFilterChangeable)
        {
            // replace
            _layers[layer].FilterChanged -= LayerFilter_FilterChanged;
            newFilterChangeable.FilterChanged += LayerFilter_FilterChanged;
            newFilterChangeable.FilterExpression = newFilterExpression;
            _layers[layer] = newFilterChangeable;
        }
        else
        {
            _layers[layer] = new Filter<T>
            {
                FilterExpression = newFilterExpression
            };
            _layers[layer].FilterChanged += LayerFilter_FilterChanged;
        }
        OnFilterChanged();
    }

    public void Add(Expression<Func<T, bool>>? predicate, FilterLayer layer = FilterLayer.Application)
    {
        if (predicate == null) // use optimistic behavior here
            return;

        var filter = new Filter<T>();
        filter.FilterExpression = predicate;
        Add(filter, layer);
    }

    public void Remove(IFilter<T> filter, FilterLayer layer = FilterLayer.User)
    {
        if (filter is FilterSet<T>)
            throw new NotSupportedException($"A FilterSet<> can not be removed from an FilterSet<> object.");

        if (_layers[layer] is IPropertyFilterCollection propertyFilterCollection &&
            filter is IPropertyFilter<T> propertyFilter)
        {
            propertyFilterCollection.Properties.Remove(propertyFilter);
        }
        else if (_layers[layer] == filter)
        {
            if (layer == FilterLayer.User)
                throw new NotSupportedException("You can not remove the whole user layer filter from an FilterSet<> object.");

            _layers.Remove(layer);
        }
        else
        {
            throw new ArgumentException("Could not find the filter in the FilterSet<> object", nameof(filter));
        }
        OnFilterChanged();
    }

    private Expression<Func<T, bool>>? ConcatenateLayerExpressions()
    {
        Expression<Func<T, bool>>? lastExpression = null;

        foreach (var layer in Layers.Values)
        {
            if (lastExpression == null)
                lastExpression = layer.FilterExpression;
            else
            {
                var currentExpression = layer.FilterExpression;
                if (currentExpression != null)
                {
                    lastExpression = lastExpression.AndAlso(currentExpression);
                }
            }
        }
            
        return lastExpression;
    }

    public override Expression<Func<T, bool>>? FilterExpression => ConcatenateLayerExpressions();

    public override void Clear()
    {
        foreach (var filter in _layers.Values)
        {
            filter.FilterChanged -= LayerFilter_FilterChanged;

            filter.Clear();

            filter.FilterChanged += LayerFilter_FilterChanged;
        }
        OnFilterChanged();
    }

    public void Clear(FilterLayer layer)
    {
        if (_layers.ContainsKey(layer))
            _layers[layer].Clear();
    }

    public static implicit operator Expression<Func<T, bool>>?(FilterSet<T>? filterSet)
    {
        return filterSet?.FilterExpression;
    }

    #region IFilterSet

    IPropertyFilterCollection IFilterSet.UserLayer => UserLayer;

    IReadOnlyDictionary<FilterLayer, IFilter> IFilterSet.Layers =>
        Layers.ToDictionary(item => item.Key, item => (IFilter) item.Value);

    void IFilterSet.Add(IFilterSet filterSet)
    {
        Add((IFilterSet<T>)filterSet);
    }

    #endregion
}