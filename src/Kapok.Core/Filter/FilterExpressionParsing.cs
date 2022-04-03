using System.Linq.Expressions;
using System.Reflection;

namespace Kapok.Core;

public static class FilterExpressionParsing
{
    /// <summary>
    /// This method will read a filter from an expression when the predicate equals a single filter like this:
    ///
    ///  (entry) => entry.PropertyName == (/* any expression goes here*/)
    /// 
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    /// <param name="filterExpression"></param>
    /// <returns></returns>
    public static IDictionary<PropertyInfo, object> ParseStaticFilters<TEntry>(Expression<Func<TEntry, bool>>? filterExpression)
        where TEntry : class, new()
    {
        var staticFilterList = new Dictionary<PropertyInfo, object>();

        if (filterExpression == null)
            return staticFilterList;

        // this code will set the property when the filter predicate equals a single filter like this:
        //  (entry) => entry.PropertyName == (/* any expression goes here*/)

        // run expression tree optimizer
        var expression = ExpressionOptimizerVisitor.Singleton.Visit(filterExpression.Body);

        ParseStaticFilters_Iteration<TEntry>(expression, staticFilterList);

        return staticFilterList;
    }

    private static void ParseStaticFilters_Iteration<TEntry>(Expression expression, IDictionary<PropertyInfo, object> staticFilterList)
        where TEntry : class, new()
    {
        switch (expression.NodeType)
        {
            case ExpressionType.Equal:
                if (expression is BinaryExpression equalBinaryExpression &&
                    equalBinaryExpression.Left is MemberExpression memberExpression &&
                    memberExpression.Member is PropertyInfo propertyInfo &&
                    memberExpression.Expression is ParameterExpression parameterExpression &&
                    parameterExpression.Type == typeof(TEntry))
                {
                    var objectMember = Expression.Convert(equalBinaryExpression.Right, typeof(object));
                    var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                    var getter = getterLambda.Compile();

                    var value = getter.Invoke();

                    staticFilterList.Add(propertyInfo, value);
                }
                break;
            case ExpressionType.AndAlso:
                if (expression is BinaryExpression andAlsoBinaryExpression)
                {
                    ParseStaticFilters_Iteration<TEntry>(andAlsoBinaryExpression.Left, staticFilterList);
                    ParseStaticFilters_Iteration<TEntry>(andAlsoBinaryExpression.Right, staticFilterList);
                }
                break;
        }
    }
}