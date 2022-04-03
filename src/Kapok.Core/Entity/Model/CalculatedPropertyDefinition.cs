using System.Linq.Expressions;
using System.Reflection;

namespace Kapok.Entity;

public interface IPropertyCalculateDefinition
{
    Expression? BuildCalculateExpression(IReadOnlyDictionary<string, object>? filterData = null);
}

public interface IPropertyCalculateDefinition<TBaseEntry, TFieldType> : IPropertyCalculateDefinition
    where TBaseEntry : class
{
    new Expression<Func<TBaseEntry, TFieldType>>? BuildCalculateExpression(IReadOnlyDictionary<string, object>? filterData = null);
}

public class PropertyCalculateDefinition<TBaseEntry, TFieldType> : IPropertyCalculateDefinition<TBaseEntry, TFieldType>
    where TBaseEntry : class
{
    private readonly MethodInfo? _dynamicCalculateFuncMethodInfo;
    private readonly Expression<Func<TBaseEntry, TFieldType>>? _calculateFunc;

    public PropertyCalculateDefinition(Expression<Func<TBaseEntry, TFieldType>> calculateFunc)
    {
        _calculateFunc = calculateFunc;
    }

    public PropertyCalculateDefinition(MethodInfo dynamicCalculateFunc)
    {
        _dynamicCalculateFuncMethodInfo = dynamicCalculateFunc;
    }

    public Expression<Func<TBaseEntry, TFieldType>>? BuildCalculateExpression(IReadOnlyDictionary<string, object>? filterData = null)
    {
        if (_dynamicCalculateFuncMethodInfo != null)
        {
            var parameters = _dynamicCalculateFuncMethodInfo.GetParameters();
            if (parameters.Length == 0)
            {
                return (Expression<Func<TBaseEntry, TFieldType>>?)_dynamicCalculateFuncMethodInfo.Invoke(null, null);
            }

            var paramValues = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == typeof(IReadOnlyDictionary<string, object>))
                {
                    paramValues[i] = filterData ?? new Dictionary<string, object>();
                }
                else
                {
                    throw new NotSupportedException(
                        "The calculation method contains an unknown type as parameter.");
                }
            }

            return (Expression<Func<TBaseEntry, TFieldType>>?)_dynamicCalculateFuncMethodInfo.Invoke(null, paramValues);
        }

        return _calculateFunc;
    }

    #region IPropertyCalculationDefinition

    Expression? IPropertyCalculateDefinition.BuildCalculateExpression(IReadOnlyDictionary<string, object>? filterData)
    {
        return BuildCalculateExpression(filterData);
    }

    #endregion
}