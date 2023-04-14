using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Kapok.Entity;
using Kapok.Entity.Model;

namespace Kapok.BusinessLayer;

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

    private static TypeInfo CreateAnonymousTypeForAutoCalculateQuery(Type entryType, IEnumerable<PropertyInfo> autoCalculatePropertyInfos)
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

        return dynamicAnonymousType.CreateTypeInfo() ?? throw new Exception("Could not create anonymous type for auto calculate property");
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
    public static IQueryable<T> AutoCalculate<T>(this IQueryable<T> query, IEnumerable<string>? properties, bool noTracking, IReadOnlyDictionary<string, object?>? nestedDataFilter = null, string[]? fields = null)
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

            TypeInfo anonymousType = CreateAnonymousTypeForAutoCalculateQuery(typeof(T), autoCalculateCache.Select(tu => tu.Item1));

            var entryParameter = Expression.Parameter(typeof(T));

            var entryFieldBinding = Expression.Bind(
#pragma warning disable CS8604
                anonymousType.GetField("__entry"),
#pragma warning restore CS8604
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
#pragma warning disable CS8604
                    member: anonymousType.GetField(propertyInfo.Name),
#pragma warning restore CS8604
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
#pragma warning disable CS8604
                    Expression.New(anonymousType.GetConstructor(Type.EmptyTypes)),
#pragma warning restore CS8604
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

#pragma warning disable CS8602
            var method = baseMethod.MakeGenericMethod(new Type[]
#pragma warning restore CS8602
            {
                typeof(T),
                anonymousType
            });

            IQueryable newQuery = query;
            QueryTranslatorProvider<T>? provider = null;
            if (query is QueryTranslator<T> queryTranslator)
            {
                // When QueryTranslator<T> is used we remove the logic
                // and implement it manually at this place. This is done because
                // we want to use the Entity Framework feature to calculate the auto-calculate columns.

                provider = (QueryTranslatorProvider<T>)queryTranslator.Provider;
                newQuery = queryTranslator.NotForUpdate<T>(provider.Source);
            }

#pragma warning disable CS8600
            newQuery = (IQueryable)method.Invoke(null, new object[] { newQuery, selectQueryExpression });
#pragma warning restore CS8600

            // With this loop we
            //   1. copy the auto-calculate columns into its main entity
            //   2. add the tracking in case it is a QueryTranslator<T> entity.
            IEnumerable<T> InternalIterateMapAutoCalculatePropertiesToEntity()
            {
#pragma warning disable CS8602
                foreach (object o in newQuery)
#pragma warning restore CS8602
                {
                    var field = anonymousType.GetField("__entry");
#pragma warning disable CS8602
#pragma warning disable CS8600
                    T trackedEntry = (T)field.GetValue(o);
#pragma warning restore CS8600
#pragma warning restore CS8602

                    foreach (var propertyInfo in autoCalculateCache.Select(p => p.Item1))
                    {
                        var calculatedField = anonymousType.GetField(propertyInfo.Name);

#pragma warning disable CS8602
                        propertyInfo.SetMethod.Invoke(trackedEntry, new[] { calculatedField.GetValue(o) });
#pragma warning restore CS8602
                    }

#pragma warning disable CS8600
                    object trackedEntryAsObject = trackedEntry;
#pragma warning restore CS8600
#pragma warning disable CS8601
                    provider?.TrackCreateIfNotAlreadyTracked(ref trackedEntryAsObject);
#pragma warning restore CS8601
#pragma warning disable CS8603
                    yield return trackedEntry;
#pragma warning restore CS8603
                }
            }

            return InternalIterateMapAutoCalculatePropertiesToEntity().AsQueryable();
        }
    }
}