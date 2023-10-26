using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using Kapok.Entity;

namespace Kapok.BusinessLayer;

public class PropertyStaticFilter : PropertyFilter, IPropertyStaticFilter
{
    public PropertyStaticFilter(Type baseType, string propertyName) : base(baseType, propertyName)
    {
    }

    public PropertyStaticFilter(Type baseType, PropertyInfo propertyInfo) : base(baseType, propertyInfo)
    {
    }

    private object? _filterValue;
    public object? FilterValue
    {
        get => _filterValue;
        set
        {
            if (SetProperty(ref _filterValue, value)
                // this is to make sure when the first value is 'null' the filter expression is parsed to 'propertyInfo.get == null'
                || FilterExpression == default
               )
            {
                FilterExpression = BuildFilterExpression(_filterValue);
            }
        }
    }

    private Expression? BuildFilterExpression(object? value)
    {
        // don't use an actual filter on nested data filtering
        if (Attribute.IsDefined(PropertyInfo, typeof(NestedDataFilterAttribute)))
            return null;

        // it does not make sense to filter on properties which are not in the database
        // TODO filtering on calculated members will be disabled with this exception
        if (Attribute.IsDefined(PropertyInfo, typeof(NotMappedAttribute)))
            return null;

        ParameterExpression[] parameters = new[]{
            Expression.Parameter(BaseType, string.Empty)
        };

        var propertyMemberExpression = Expression.Property(parameters[0], PropertyInfo);

        if (value != null)
        {
            if (!PropertyInfo.PropertyType.IsAssignableFrom(value.GetType()))
            {
                throw new NotSupportedException(string.Format("The static value type {0} is not assignable to the property type {1}.", value.GetType(), PropertyInfo.PropertyType));
            }
        }

        return Expression.Lambda(
            Expression.Equal(
                propertyMemberExpression,
                Expression.Constant(value, PropertyInfo.PropertyType)
            )
            , parameters);
    }

    public override void Clear()
    {
        _filterValue = null;
        FilterExpression = null;
    }
}

public class PropertyStaticFilter<T> : PropertyFilter<T>, IPropertyStaticFilter<T>
    where T : class
{
    public PropertyStaticFilter(string propertyName) : base(propertyName)
    {
    }

    public PropertyStaticFilter(PropertyInfo propertyInfo) : base(propertyInfo)
    {
    }

    private object? _filterValue;
    public object? FilterValue
    {
        get => _filterValue;
        set
        {
            if (SetProperty(ref _filterValue, value)
                // this is to make sure when the first value is 'null' the filter expression is parsed to 'propertyInfo.get == null'
                || FilterExpression == default
               )
            {
                FilterExpression = BuildFilterExpression(_filterValue);
            }
        }
    }

    private Expression<Func<T, bool>>? BuildFilterExpression(object? value)
    {
        // don't use an actual filter on nested data filtering
        if (Attribute.IsDefined(PropertyInfo, typeof(NestedDataFilterAttribute)))
            return null;

        // it does not make sense to filter on properties which are not in the database
        // TODO filtering on calculated members will be disabled with this exception
        if (Attribute.IsDefined(PropertyInfo, typeof(NotMappedAttribute)))
            return null;

        ParameterExpression[] parameters = new[]{
            Expression.Parameter(typeof(T), string.Empty)
        };

        var propertyMemberExpression = Expression.Property(parameters[0], PropertyInfo);

        if (value != null)
        {
            if (!PropertyInfo.PropertyType.IsAssignableFrom(value.GetType()))
            {
                throw new NotSupportedException(string.Format("The static value type {0} is not assignable to the property type {1}.", value.GetType(), PropertyInfo.PropertyType));
            }
        }

        return (Expression<Func<T, bool>>)Expression.Lambda(
            Expression.Equal(
                propertyMemberExpression,
                Expression.Constant(value, PropertyInfo.PropertyType)
            )
            , parameters);
    }

    public override void Clear()
    {
        _filterValue = null;
        FilterExpression = null;
    }
}