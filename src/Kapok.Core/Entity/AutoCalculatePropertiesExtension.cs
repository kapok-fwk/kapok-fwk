using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Kapok.Entity;
using Kapok.Entity.Model;

namespace Kapok.Core;

public static class AutoCalculatePropertiesExtension
{
    class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly Expression _newExpression;

        private readonly ParameterExpression _oldParameter;

        private ParameterReplaceVisitor(ParameterExpression oldParameter, Expression newExpression)
        {
            _oldParameter = oldParameter;
            _newExpression = newExpression;
        }

        internal static Expression Replace(Expression expression, ParameterExpression oldParameter,
            Expression newExpression)
        {
            return new ParameterReplaceVisitor(oldParameter, newExpression).Visit(expression);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == _oldParameter)
                return _newExpression;

            return base.VisitParameter(node);
        }
    }

    private static TypeInfo? CreateAnonymousTypeForAutoCalculateQuery(Type entryType, IEnumerable<PropertyInfo> autoCalculatePropertyInfos)
    {
        var assemblyName = new AssemblyName("TempAssembly");
        var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var dynamicModule = dynamicAssembly.DefineDynamicModule("TempModule");

        var dynamicAnonymousType = dynamicModule.DefineType("AnonymousType", TypeAttributes.Public);

        dynamicAnonymousType.DefineField("__entry", entryType, FieldAttributes.Public);

        foreach (var propertyInfo in autoCalculatePropertyInfos)
        {
            dynamicAnonymousType.DefineField(propertyInfo.Name, propertyInfo.PropertyType, FieldAttributes.Public);
        }

        return dynamicAnonymousType.CreateTypeInfo();
    }

    /// <summary>
    /// This method will auto-calculate all properties given in the parameter properties.
    /// 
    /// When the parameter fields is not null, only the given fields will be taken from the database.
    /// 
    /// NOTE: The use of this function will disable the Entity Framework change tracker. When you use this
    /// function and you need the tracking, you need to add the entries manually to the change tracker.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="properties"></param>
    /// <param name="noTracking"></param>
    /// <param name="nestedDataFilter"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    public static IQueryable<T> AutoCalculate<T>(this IQueryable<T> query, IEnumerable<string>? properties, bool noTracking, IReadOnlyDictionary<string, object>? nestedDataFilter = null, string[]? fields = null)
        where T : class, new()
    {
        if (properties == null)
            return query;

        IEntityModel model = EntityBase.GetEntityModel<T>();
        if (model == null)
            throw new NotSupportedException("The generic type T must implement the interface IEntityModelStore");

        var autoCalculateCache = new List<Tuple<PropertyInfo, IPropertyCalculateDefinition>>();

        foreach (var propertyName in properties)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty |
                BindingFlags.GetProperty);
            var propertyModel = model.Properties.FirstOrDefault(p => p.PropertyName == propertyName);
                        
            if (propertyInfo != null && propertyModel != null && propertyModel.CalculateDefinition != null &&
                    
                // optimistic behavior here: when a auto-calc. property is given twice, we will only take it once into the auto-calculation property list cache
                autoCalculateCache.All(tup => tup.Item1 != propertyInfo))
            {
                if (propertyInfo.SetMethod == null)
                    throw new NotSupportedException($"The auto-calculate property {propertyInfo.Name} does not implement the property-set method. An auto-calculate property must implement the get and set method.");

                autoCalculateCache.Add(new Tuple<PropertyInfo, IPropertyCalculateDefinition>(
                    item1: propertyInfo,
                    item2: propertyModel.CalculateDefinition
                ));
            }
        }

        if (autoCalculateCache.Count == 0)
            return query;

        // create Linq query for calculated members

        // I requested an solution from the EF Core team which combines option 1 and option 2:
        //   https://github.com/aspnet/EntityFrameworkCore/issues/15509

        // TODO: consider if there is still an requirement to do here wto different executions after upgrading to EF Core 3.1, since both queries are executed likewise

        // option 1 runs without change tracker. This option supports 'field' parameter.
        if (noTracking)
        {
            if (fields == null)
            {
                fields = (
                    from property in typeof(T).GetProperties(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                        BindingFlags.SetProperty)
                    where !Attribute.IsDefined(property, typeof(NotMappedAttribute)) &&

                          // create the list from all fields which have a default type
                          (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                    select property.Name
                ).ToArray();
            }

            var entryParameter = Expression.Parameter(typeof(T));
            var primaryKeyBindings = (from property in typeof(T).GetProperties(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty)
                where !Attribute.IsDefined(property, typeof(NotMappedAttribute)) &&
                      fields.Contains(property.Name)
                select Expression.Bind(property,
                    Expression.Property(entryParameter, property)
                )).ToList();

            List<MemberAssignment> calculatedPropertiesBinding = new List<MemberAssignment>();
            foreach (var cacheData in autoCalculateCache)
            {
                var propertyInfo = cacheData.Item1;
                var calculateDefinition = cacheData.Item2;

                var calculateFuncLambda = calculateDefinition.BuildCalculateExpression(nestedDataFilter) as LambdaExpression;
                if (calculateFuncLambda == null)
                    throw new NotSupportedException($"Could not convert the auto-calculate expression from property {propertyInfo.Name} to a lambda expression.");

                var newBinding = Expression.Bind(propertyInfo,
                    ParameterReplaceVisitor.Replace(
                        calculateFuncLambda.Body
                        , calculateFuncLambda.Parameters[0]
                        , entryParameter
                    )
                );
                calculatedPropertiesBinding.Add(newBinding);
            }

            var selectQueryExpression = Expression.Lambda(
                Expression.MemberInit(
                    Expression.New(typeof(T)),
                    primaryKeyBindings.Concat(calculatedPropertiesBinding)
                )
                , entryParameter
            );

            // build new query
            query = query.Select((Expression<Func<T, T>>)selectQueryExpression);
            var optimizedQueryExpression = ExpressionOptimizerVisitor.Singleton.Visit(query.Expression);
            if (optimizedQueryExpression == null)
                throw new NotSupportedException("Could not optimize the expression.");
            query = query.Provider.CreateQuery<T>(optimizedQueryExpression);

            return query;
        }
        // option 2 runs with change tracker but iterates each auto-calculated field in an own SQL statement. This option does not support the 'field' parameter.
        else
        {
            if (fields != null)
                throw new NotSupportedException(
                    $"When parameter {nameof(fields)} is given the option {nameof(noTracking)} must be true.");

            var anonymousType = CreateAnonymousTypeForAutoCalculateQuery(typeof(T), autoCalculateCache.Select(tu => tu.Item1));

            var entryParameter = Expression.Parameter(typeof(T));

            var entryFieldBinding = Expression.Bind(
                anonymousType.GetField("__entry"),
                entryParameter
            );

            List<MemberAssignment> calculatedPropertiesBinding = new List<MemberAssignment>();
            foreach (var cacheData in autoCalculateCache)
            {
                var propertyInfo = cacheData.Item1;
                var calculateDefinition = cacheData.Item2;

                var calculateFuncLambda = calculateDefinition.BuildCalculateExpression(nestedDataFilter) as LambdaExpression;
                if (calculateFuncLambda == null)
                    throw new NotSupportedException($"Could not convert the auto-calculate expression from property {propertyInfo.Name} to a lambda expression.");

                var newBinding = Expression.Bind(
                    member: anonymousType.GetField(propertyInfo.Name),
                    expression: ParameterReplaceVisitor.Replace(
                        calculateFuncLambda.Body
                        , calculateFuncLambda.Parameters[0]
                        , entryParameter
                    )
                );
                calculatedPropertiesBinding.Add(newBinding);
            }

            var selectQueryExpression = Expression.Lambda(
                Expression.MemberInit(
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Expression.New(anonymousType.GetConstructor(Type.EmptyTypes)),
                    new[] { entryFieldBinding }.Concat(calculatedPropertiesBinding)
                )
                , entryParameter
            );

            // build new query

            // search for method: public static IQueryable<TResult> Select<TSource, TResult>(this IQueryable<TSource> source, Expression< Func < TSource, TResult >> selector)
            var baseMethod = typeof(Queryable).GetMethodExt(nameof(Queryable.Select),
                BindingFlags.Static | BindingFlags.Public, new Type[]
                {
                    typeof(IQueryable<>),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>))
                });

            var method = baseMethod.MakeGenericMethod(new Type[]
            {
                typeof(T),
                anonymousType
            });

            var newQuery = (IQueryable)method.Invoke(null, new object[] { query, selectQueryExpression });

            QueryTranslatorProvider<T> provider = null;

            if (query is QueryTranslator<T> queryTranslator)
            {
                // When DeferredDao<T>.QueryTranslator<T> is used we remove the logic
                // and implement it manually at this place. This is done because
                // we want to use the Entity Framework feature to calculate the auto-calculate columns.

                provider = (QueryTranslatorProvider<T>)queryTranslator.Provider;
                newQuery = newQuery.NotForUpdate<T>(provider._source);
            }

            // With this loop we
            //   1. copy the auto-calculate columns into its main entity
            //   2. add the tracking in case it is a QueryTranslator<T> entity.
            //
            // NOTE: probably this whole logic could be put in an own enumerable-like IQueryProvider so we don't have to
            //       iterate over it here when e.g. it is never requested in the aftermath.
            var newResultList = new List<T>();
            foreach (object o in newQuery)
            {
                var field = anonymousType.GetField("__entry");
                T trackedEntry = (T)field.GetValue(o);

                foreach (var propertyInfo in autoCalculateCache.Select(p => p.Item1))
                {
                    var calculatedField = anonymousType.GetField(propertyInfo.Name);

                    propertyInfo.SetMethod.Invoke(trackedEntry, new[] {calculatedField.GetValue(o)});
                }

                newResultList.Add(trackedEntry);
                object trackedEntryAsObject = trackedEntry;
                provider?.TrackCreateIfNotAlreadyTracked(ref trackedEntryAsObject);
            }

            return newResultList.AsQueryable();
        }
    }
}