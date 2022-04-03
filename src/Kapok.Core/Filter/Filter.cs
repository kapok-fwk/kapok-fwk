using System.Linq.Expressions;

namespace Kapok.Core;

public class Filter<T> : FilterBase<T>
{
    public Filter()
    {
    }

    public Filter(Expression<Func<T, bool>>? expression)
    {
        FilterExpression = expression;
    }

    public new Expression<Func<T, bool>>? FilterExpression
    {
        get => base.FilterExpression;
        set
        {
            if (base.FilterExpression == value) return;
            base.FilterExpression = value;
            OnFilterChanged();
        }
    }

    public override void Clear()
    {
        FilterExpression = null;
    }
}