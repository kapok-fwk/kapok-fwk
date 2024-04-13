using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.Entity.Model;

public class ReferenceModelBuilder<T, TDestinationType>
    where T : class
    where TDestinationType : class, new()

{
    private readonly EntityRelationship _entityReference;
    private readonly EntityModel? _model;

    internal ReferenceModelBuilder(EntityRelationship entityReference, EntityModel? model = null)
    {
        _entityReference = entityReference;
        _model = model;
    }

    public ReferenceModelBuilder<T, TDestinationType> OnDelete(DeleteBehavior deleteBehavior = DeleteBehavior.Restrict)
    {
        _entityReference.DeleteBehavior = deleteBehavior;

        return this;
    }

    public ReferenceModelBuilder<T, TDestinationType> HasForeignKey(params string[] parameterNames)
    {
        var propertyInfoList = EntityModelBuilder<T>.GetPropertyInfosFromNames(typeof(T), parameterNames);

        _entityReference.ForeignKeyProperties = propertyInfoList.ToList();

        return this;
    }

    public ReferenceModelBuilder<T, TDestinationType> HasPrincipalKey(params string[] parameterNames)
    {
        var propertyInfoList = EntityModelBuilder<T>.GetPropertyInfosFromNames(typeof(TDestinationType), parameterNames);

        _entityReference.PrincipalKeyProperties = propertyInfoList.ToList();

        return this;
    }

    public ReferenceModelBuilder<T, TDestinationType> WithForeignNavigationProperty(string parameterName)
    {
        var propertyInfo = typeof(TDestinationType).GetProperty(parameterName, BindingFlags.Public | BindingFlags.Instance);
        if (propertyInfo == null)
            throw new ArgumentException($"Could not find property with name {parameterName} in type {typeof(T).FullName}");

        _entityReference.ForeignNavigationProperty = propertyInfo;

        return this;
    }

    public ReferenceModelBuilder<T, TDestinationType> WithPrincipalNavigationProperty(string parameterName)
    {
        var propertyInfo = typeof(T).GetProperty(parameterName, BindingFlags.Public | BindingFlags.Instance);
        if (propertyInfo == null)
            throw new ArgumentException($"Could not find property with name {parameterName} in type {typeof(TDestinationType).FullName}");

        _entityReference.PrincipalNavigationProperty = propertyInfo;

        return this;
    }

    public void AddLookupToLastProperty()
    {
        if (_model == null)
            throw new NotSupportedException($"The class {typeof(ReferenceModelBuilder<,>).FullName} needs to be initialized with model to use method {nameof(AddLookupToLastProperty)}");

        if (_entityReference.ForeignKeyProperties == null || _entityReference.ForeignKeyProperties.Count == 0)
            throw new NotSupportedException("Foreign key property not set");

        var entityDestinationModel = EntityBase.GetEntityModel<TDestinationType>();
        if (entityDestinationModel.PrimaryKeyProperties == null ||
            entityDestinationModel.PrimaryKeyProperties.Length == 0)
            throw new NotSupportedException($"The referenced entity {typeof(TDestinationType).FullName} does not have a primary key. You can't use this entity with function {nameof(AddLookupToLastProperty)}.");

        if (entityDestinationModel.PrimaryKeyProperties.Length != _entityReference.ForeignKeyProperties.Count)
            throw new NotSupportedException($"The number of properties between primary key ({entityDestinationModel.PrimaryKeyProperties.Length}) and foreign key ({_entityReference.ForeignKeyProperties.Count}) in the reference is not equal.");

        var foreignKeyLastPropertyInfo = _entityReference.ForeignKeyProperties.Last();
        var principalKeyLastPropertyInfo = entityDestinationModel.PrimaryKeyProperties.Last();

        var entityBuilder = new EntityModelBuilder<T>(_model);
        var propertyBuilder = entityBuilder.GetProperty(foreignKeyLastPropertyInfo);

        // remove data partition properties from the list

        int primaryKeyPropertyId = 0;
        // TODO: Investigate if not the specific data domain can be used here
        foreach (var dataPartition in DataDomain.Default?.DataPartitions.Values ?? new List<DataPartition>())
        {
            if (dataPartition.InterfaceType.IsAssignableFrom(typeof(T)) &&
                // We only do the data partition skipping when the primary key column has the same name as the column defined in the interface
                // and when they appear in the same order as they are registered to the DataDomain
                _entityReference.ForeignKeyProperties[primaryKeyPropertyId].Name == dataPartition.PartitionProperty.Name)
            {
                primaryKeyPropertyId++;

                // makes sure that we don't run over the @this.Model.PrimaryKeyProperties array length
                if (_entityReference.ForeignKeyProperties.Count == primaryKeyPropertyId + 1)
                    break;
            }
        }

        // all foreign key properties which are not part of a data partition (=> local data)
        var localForeignKeyProperties = new PropertyInfo[_entityReference.ForeignKeyProperties.Count - primaryKeyPropertyId];
        if (localForeignKeyProperties.Length == 0)
            throw new NotSupportedException(
                "All foreign key properties are part of a data partition and can therefore not be used as lookup property (!)");

        _entityReference.ForeignKeyProperties.CopyTo(primaryKeyPropertyId, localForeignKeyProperties, 0, _entityReference.ForeignKeyProperties.Count - primaryKeyPropertyId);

        Delegate lookup;
        MethodInfo addLookupMethod;
        if (localForeignKeyProperties.Length == 1)
        {
            // simple lookup expression
            Func<IDataDomainScope, IQueryable<TDestinationType>> lookupFunc =
                dds => dds.GetEntityService<TDestinationType>().AsQueryable();
            lookup = lookupFunc;

            addLookupMethod = (
                    from m in typeof(PropertyModelBuilder<T>).GetMethods(
                        BindingFlags.Public | BindingFlags.Instance)
                    where m.Name == nameof(PropertyModelBuilder<T>.AddLookup) &&
                          m.IsGenericMethodDefinition &&
                          m.GetGenericArguments().Length == 2 &&
                          m.GetParameters().Length == 2 &&
                          m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)
                    select m).Single()
                .MakeGenericMethod(
                    typeof(TDestinationType),
                    foreignKeyLastPropertyInfo.PropertyType
                );

        }
        else // use 'where' clause for other property fields
        {
            var currentParameter = Expression.Parameter(typeof(T), "current");
            var ddsParameter = Expression.Parameter(typeof(IDataDomainScope), "dds");
            var getEntityServiceMethod = typeof(IDataDomainScope).GetMethod(nameof(IDataDomainScope.GetEntityService), 1, Type.EmptyTypes)
                ?.MakeGenericMethod(typeof(TDestinationType));
            Debug.Assert(getEntityServiceMethod != null);
            var asQueryableMethod = typeof(IEntityReadOnlyService<>).MakeGenericType(typeof(TDestinationType))
                .GetMethod(nameof(IEntityReadOnlyService<object>.AsQueryable));
            Debug.Assert(asQueryableMethod != null);
            var whereLinqMethodInfo = (
                    from m in typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    where m.Name == nameof(Queryable.Where) &&
                          m.IsGenericMethodDefinition &&
                          m.GetGenericArguments().Length == 1 &&
                          m.GetParameters().Length == 2 &&
                          m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>) &&
                          m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericTypeDefinition() ==
                          typeof(Func<,>)
                    select m).Single()
                .MakeGenericMethod(typeof(TDestinationType));

            // build lambda expression for e => e.Property == current.Property),
            var whereLambdaExpression = _entityReference.GenerateChildrenWherePartExpression(currentParameter);
            if (whereLambdaExpression == null)
                throw new Exception($"Could not generate where expression for entity reference {_entityReference.Name}");

            // create dds => dds.GetEntityService<TDestinationType>().AsQueryable().Where(.. condition for fields ..)
            LambdaExpression lookupExpression =
                Expression.Lambda(
                    Expression.Call(
                        null,
                        whereLinqMethodInfo,
                        Expression.Call(
                            Expression.Call(ddsParameter, getEntityServiceMethod)
                            , asQueryableMethod
                        ),
                        whereLambdaExpression
                    )
                    , currentParameter
                    , ddsParameter
                );
            lookup = lookupExpression.Compile();
                
            addLookupMethod = (
                    from m in typeof(PropertyModelBuilder<T>).GetMethods(
                        BindingFlags.Public | BindingFlags.Instance)
                    where m.Name == nameof(PropertyModelBuilder<T>.AddLookup) &&
                          m.IsGenericMethodDefinition &&
                          m.GetGenericArguments().Length == 2 &&
                          m.GetParameters().Length == 2 &&
                          m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<,,>)
                    select m).Single()
                .MakeGenericMethod(
                    typeof(TDestinationType),
                    foreignKeyLastPropertyInfo.PropertyType
                );
        }
            
        // field selector expression
        var entityParam = Expression.Parameter(typeof(TDestinationType), "e");

        LambdaExpression fieldSelectorExpression;
        if (principalKeyLastPropertyInfo.PropertyType == foreignKeyLastPropertyInfo.PropertyType)
        {
            // type matches
            fieldSelectorExpression =
                Expression.Lambda(
                    Expression.Property(entityParam, principalKeyLastPropertyInfo),
                    entityParam
                );
        }
        else
        {
            if (foreignKeyLastPropertyInfo.PropertyType.IsGenericType &&
                foreignKeyLastPropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                foreignKeyLastPropertyInfo.PropertyType.GenericTypeArguments[0] == principalKeyLastPropertyInfo.PropertyType)
            {
                // cast of e.g. (int?)value where value : int
                fieldSelectorExpression =
                    Expression.Lambda(
                        Expression.Convert(
                            Expression.Property(entityParam, principalKeyLastPropertyInfo),
                            foreignKeyLastPropertyInfo.PropertyType
                        ),
                        entityParam
                    );
            }
            else
            {
                throw new NotSupportedException($"Unexpected type cast in reference: principal key last field type {principalKeyLastPropertyInfo.PropertyType.FullName} and foreign key last field type {foreignKeyLastPropertyInfo.PropertyType.FullName}");
            }
        }

        addLookupMethod.Invoke(propertyBuilder, 
            new object[]
            {
                lookup,
                fieldSelectorExpression
            });
    }
}