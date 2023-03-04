using Kapok.BusinessLayer;

namespace Kapok.Entity.Model;

public interface IDrillDownDefinition
{
    Type PageType { get; }

    // TODO: replace Action<IFilterSet, object, IReadOnlyDictionary<string, object>> with an filter builder class
    /// <summary>
    /// An procedure which sets the filter to the drop-down data.
    ///
    /// 1. IFilterSet is the dataSet for the filter of the drill-down data.
    /// 2. object is the entity which was selected on drill-down.
    /// 3. IReadOnlyDictionary&lt;string,object&gt; is a list of all static filters applied to properties on the object which is drilled-down (this list will contain e.g. flow-filters which shall be passed over to the detail data).
    /// </summary>
    Action<IFilterSet, object, IReadOnlyDictionary<string, object?>> Filter { get; }

    /* TODO: not implemented yet
    /// <summary>
    /// If this field is set, the PageType and Filter is ignored.
    /// </summary>
    //Action DrillDownAction { get; }
    */
}

public interface IDrillDownDefinition<TDestinationEntry, in TSourceEntry> : IDrillDownDefinition
    where TDestinationEntry : class
    where TSourceEntry : class
{
    new Action<IFilterSet<TDestinationEntry>, TSourceEntry, IReadOnlyDictionary<string, object?>> Filter { get; }
}

public class DrillDownDefinition : IDrillDownDefinition
{
    public DrillDownDefinition(Type pageType, Action<IFilterSet, object, IReadOnlyDictionary<string, object?>> filter)
    {
        PageType = pageType;
        Filter = filter;
    }

    public Type PageType { get; protected set; }
    public Action<IFilterSet, object, IReadOnlyDictionary<string, object?>> Filter { get; protected set; }
    public Action? DrillDownAction { get; protected set; }
}

public class DrillDownDefinition<TDestinationEntry, TSourceEntry> : DrillDownDefinition, IDrillDownDefinition<TDestinationEntry, TSourceEntry>
    where TDestinationEntry : class
    where TSourceEntry : class
{
    public DrillDownDefinition(Type pageType,
        Action<IFilterSet<TDestinationEntry>, TSourceEntry, IReadOnlyDictionary<string, object?>> filter)
        : base(
            pageType,
            (filterSet, entry, entryStaticFilters) => filter.Invoke((IFilterSet<TDestinationEntry>)filterSet,
                (TSourceEntry)entry, entryStaticFilters)
        )
    {
    }

    Action<IFilterSet<TDestinationEntry>, TSourceEntry, IReadOnlyDictionary<string, object?>> IDrillDownDefinition<TDestinationEntry, TSourceEntry>.
        Filter => Filter;
}