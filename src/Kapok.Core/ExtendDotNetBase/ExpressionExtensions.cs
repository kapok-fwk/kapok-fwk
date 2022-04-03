namespace System.Linq.Expressions;

public static class ExpressionExtensions
{
    public static string GetMemberName<T>(this Expression<T> expression)
    {
        switch (expression.Body)
        {
            case MemberExpression m:
                return m.Member.Name;
            case UnaryExpression u when u.Operand is MemberExpression m:
                return m.Member.Name;
            default:
                throw new NotSupportedException(expression.GetType().ToString());
        }
    }

    // source: https://stackoverflow.com/questions/457316/combining-two-expressions-expressionfunct-bool
    public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);

        var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);

        if (left == null)
            throw new ArgumentException($"Could not replace the parameter of {nameof(expr1)}.", nameof(expr1));
        if (right == null)
            throw new ArgumentException($"Could not replace the parameter of {nameof(expr2)}.", nameof(expr2));

        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(left, right), parameter);
    }

    private class ReplaceExpressionVisitor
        : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression? Visit(Expression? node)
        {
            if (node == _oldValue)
                return _newValue;
            return base.Visit(node);
        }
    }
}