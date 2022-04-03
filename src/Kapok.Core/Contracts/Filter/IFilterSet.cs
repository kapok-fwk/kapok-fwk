using System.Linq.Expressions;

namespace Kapok.Core;

public interface IFilterSet : IFilter
{
    IReadOnlyDictionary<FilterLayer, IFilter> Layers { get; }

    IPropertyFilterCollection UserLayer { get; }

    void Add(IFilterSet filterSet);
}

public interface IFilterSet<TEntry> : IFilter<TEntry>, IFilterSet
    where TEntry : class
{
    new IReadOnlyDictionary<FilterLayer, IFilter<TEntry>> Layers { get; }

    void Add(IFilterSet<TEntry> filterSet);
    void Add(IFilter<TEntry> filter, FilterLayer layer = FilterLayer.User);
    void Add(Expression<Func<TEntry, bool>> predicate, FilterLayer layer = FilterLayer.Application);
    void Remove(IFilter<TEntry> filter, FilterLayer layer = FilterLayer.User);

    /// <summary>
    /// Clear a specific filter layer.
    /// </summary>
    /// <param name="layer"></param>
    void Clear(FilterLayer layer);
}