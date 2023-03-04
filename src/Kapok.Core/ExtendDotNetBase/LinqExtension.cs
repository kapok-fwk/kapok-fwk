using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq;

public static class LinqExtension
{
    #region IQueryable

    public static T GetNext<T>(this IQueryable<T> queryable, T current)
    {
        return queryable.SkipWhile(i => !(i == null ? Equals(i, current) : i.Equals(current))).Skip(1).First();
    }

    public static T GetPrevious<T>(this IQueryable<T> queryable, T current)
    {
        return queryable.TakeWhile(x => !(x == null ? Equals(x, current) : x.Equals(current))).Last();
    }

    public static T? GetNextOrDefault<T>(this IQueryable<T> queryable, T current)
    {
        return queryable.SkipWhile(i => !(i == null ? Equals(i, current) : i.Equals(current))).Skip(1).FirstOrDefault();
    }

    public static T? GetPreviousOrDefault<T>(this IQueryable<T> queryable, T current)
    {
        return queryable.TakeWhile(x => !(x == null ? Equals(x, current) : x.Equals(current))).LastOrDefault();
    }

    public static IQueryable<T> Clone<T>(this IQueryable<T> queryable)
        where T : ICloneable
    {
        return queryable.Select(item => (T)item.Clone());
    }

    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> queryable, IReadOnlyList<PropertyInfo> propertyInfos)
        where T : class
    {
        if (propertyInfos.Count == 0)
            throw new ArgumentOutOfRangeException($"The parameter {propertyInfos} cannot have ane empty list");

        var type = typeof(T);
        var param = Expression.Parameter(type);

        IOrderedQueryable<T>? orderedQueryable = null;

        foreach (var propertyInfo in propertyInfos)
        {
            var orderExpression = (Expression<Func<T, object>>)Expression.Lambda(
                Expression.Convert(
                    Expression.Property(param, propertyInfo),
                    typeof(object)
                ),
                param
            );

            orderedQueryable = orderedQueryable == null
                ? queryable.OrderBy(orderExpression)
                : orderedQueryable.ThenBy(orderExpression);
        }

#pragma warning disable CS8603
        return orderedQueryable;
#pragma warning restore CS8603
    }

    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> queryable, IReadOnlyList<PropertyInfo> propertyInfos)
        where T : class
    {
        if (propertyInfos.Count == 0)
            throw new ArgumentOutOfRangeException($"The parameter {propertyInfos} cannot have ane empty list");

        var type = typeof(T);
        var param = Expression.Parameter(type);

        IOrderedQueryable<T>? orderedQueryable = null;

        foreach (var propertyInfo in propertyInfos) 
        {
            var orderExpression = (Expression<Func<T, object>>)Expression.Lambda(
                Expression.Convert(
                    Expression.Property(param, propertyInfo),
                    typeof(object)
                ),
                param
            );

            orderedQueryable = orderedQueryable == null
                ? queryable.OrderByDescending(orderExpression)
                : orderedQueryable.ThenByDescending(orderExpression);
        }

#pragma warning disable CS8603
        return orderedQueryable;
#pragma warning restore CS8603
    }

    #endregion

    #region IEnumerable

    public static T GetNext<T>(this IEnumerable<T> queryable, T current)
    {
        return queryable.SkipWhile(i => !(i == null ? Equals(i, current) : i.Equals(current))).Skip(1).First();
    }

    public static T GetPrevious<T>(this IEnumerable<T> queryable, T current)
    {
        return queryable.TakeWhile(x => !(x == null ? Equals(x, current) : x.Equals(current))).Last();
    }

    public static T? GetNextOrDefault<T>(this IEnumerable<T> queryable, T current)
    {
        return queryable.SkipWhile(i => !(i == null ? Equals(i, current) : i.Equals(current))).Skip(1).FirstOrDefault();
    }

    public static T? GetPreviousOrDefault<T>(this IEnumerable<T> queryable, T current)
    {
        return queryable.TakeWhile(x => !(x == null ? Equals(x, current) : x.Equals(current))).LastOrDefault();
    }
        
    public static IEnumerable<T> Clone<T>(this IEnumerable<T> enumerable)
        where T : ICloneable
    {
        if (enumerable is List<T> list)
        {
            return list.ConvertAll(CloneConverter);
        }

        return enumerable.Select(item => (T)item.Clone());
    }

    private static T CloneConverter<T>(T item)
        where T : ICloneable
    {
        return (T)item.Clone();
    }

    #endregion
}