using System.Linq.Expressions;

namespace Kapok.Core;

public abstract class FilterBase<T> : IFilter<T>
{
    public virtual Expression<Func<T, bool>>? FilterExpression { get; protected set; }

    public event EventHandler? FilterChanged;

    public abstract void Clear();

    // ReSharper disable once CollectionNeverQueried.Local
    private readonly List<DeferFilterChangeObject> _deferFilterChange = new();

    protected virtual void OnFilterChanged()
    {
        if (_deferFilterChange.Count > 0)
            return; // is deferred

        FilterChanged?.Invoke(this, EventArgs.Empty);
    }

    private class DeferFilterChangeObject : IDisposable
    {
        private FilterBase<T>? _filter;

        public DeferFilterChangeObject(FilterBase<T> filter)
        {
            _filter = filter;
            _filter._deferFilterChange.Add(this);
        }

        public void Dispose()
        {
            if (_filter == null) return;
            _filter._deferFilterChange.Remove(this);
            _filter.OnFilterChanged();
            _filter = null;
        }
    }

    public IDisposable DeferFilterChange()
    {
        return new DeferFilterChangeObject(this);
    }

    #region Explicit IFilter

    Expression? IFilter.FilterExpression => FilterExpression;
        
    #endregion
}