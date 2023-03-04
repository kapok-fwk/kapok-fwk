using System.Linq.Expressions;

namespace Kapok.BusinessLayer;

public interface IFilter<T> : IFilter
{
    // NOTE: In theory we could use Expression<Predicate<T>> which would give more insight what the function is allowed to do, but since Linq doesn't support this, it is here not implemented.
    new Expression<Func<T, bool>>? FilterExpression { get; }
}
    
public interface IFilter : INotifyFilterChanged
{
    Expression? FilterExpression { get; }

    /// <summary>
    /// Clears the current filter.
    /// </summary>
    void Clear();
}