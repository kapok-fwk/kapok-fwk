using System.Linq.Expressions;
using System.Reflection;

namespace Kapok.BusinessLayer;

public enum FilterExpressionModifierAction
{
    SetFilterValue,
    GetFilterValue,
    RemoveFilter
}

public class FilterExpressionModifier : ExpressionVisitor
{
    public FilterExpressionModifierAction Action { get; }

    public FilterExpressionModifier(FilterExpressionModifierAction action, Type? baseParameterType, PropertyInfo parameterPropertyInfo)
    {
        Action = action;
        ParameterPropertyInfo = parameterPropertyInfo;
        BaseParameterType = baseParameterType;
    }

    public bool FoundFilter { get; private set; }

    public PropertyInfo ParameterPropertyInfo { get; }

    /// <summary>
    /// The type of the parameter of the base type of <see cref="ParameterPropertyInfo">ParameterMemberInfo</see>
    /// </summary>
    public Type? BaseParameterType { get; set; }

    public object? ParameterValue { get; set; }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (!FoundFilter)
        {
            if (node.NodeType == ExpressionType.Equal)
            {
                if (node.Left.NodeType == ExpressionType.MemberAccess &&
                    node.Left is MemberExpression memberExpression &&
                    memberExpression.Member == ParameterPropertyInfo)
                {
                    // we found the CompanyNum assignment, lets handle the value of the right side
                    FoundFilter = true;

                    if (Action == FilterExpressionModifierAction.GetFilterValue)
                        ParameterValue = Expression.Lambda(node.Right).Compile().DynamicInvoke();
                    else if (Action == FilterExpressionModifierAction.SetFilterValue)
                    {
                        var newValueExpression = Expression.Constant(ParameterValue, ParameterPropertyInfo.PropertyType);

                        return Expression.Equal(node.Left, newValueExpression);
                    }
                    else if (Action == FilterExpressionModifierAction.RemoveFilter)
                    {
                        return Expression.Constant(true);
                    }
                }
            }
        }

        return base.VisitBinary(node);
    }

    private bool _innerCall;

    public override Expression? Visit(Expression? node)
    {
        Expression? expression;
        if (_innerCall == false)
        {
            _innerCall = true;
            expression = base.Visit(node);
            _innerCall = false;

            if (!FoundFilter && Action == FilterExpressionModifierAction.SetFilterValue &&
                expression is LambdaExpression lambdaExpression)
            {
                // At this point nothing has been overridden in the lambda expression, so we add '&& param.property = value' to the expression

                ParameterExpression? baseParameterExpression = null;
                foreach (var parameterExpression in lambdaExpression.Parameters)
                {
                    if (parameterExpression.Type == BaseParameterType)
                    {
                    
                        baseParameterExpression = parameterExpression;
                    }
                }

                if (baseParameterExpression == null)
                {
                    if (BaseParameterType == null)
                        throw new NotSupportedException(
                            $"The expression modifier could not find anything to override and {nameof(BaseParameterType)} was not set.");

                    throw new NotSupportedException(
                        $"The expression modifier could not find anything ot override and no parameter of the base type {BaseParameterType.FullName} could be found.");
                }

                var newValueExpression = Expression.Constant(ParameterValue, ParameterPropertyInfo.PropertyType);

                var equalExpression = Expression.Equal(
                    Expression.Property(baseParameterExpression, ParameterPropertyInfo)
                    ,
                    newValueExpression
                );

                var newBodyExpression =
                    Expression.And(lambdaExpression.Body, equalExpression);

                expression = Expression.Lambda(newBodyExpression, lambdaExpression.Parameters);
            }
        }
        else
        {
            expression = base.Visit(node);
        }

        return expression;
    }
}