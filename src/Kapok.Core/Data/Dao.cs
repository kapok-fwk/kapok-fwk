using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kapok.Data;
using Kapok.Entity.Model;

namespace Kapok.BusinessLayer;

public class Dao<T> : DaoBase<T>
    where T : class, new()
{
    /// <summary>
    /// The repository the DAO is bound to.
    /// </summary>
    protected readonly IRepository<T> Repository;

    private bool _isReadOnly;

    public Dao(IDataDomainScope dataDomainScope, IRepository<T> repository, bool isReadOnly = false)
        : base(dataDomainScope)
    {
        Repository = repository;
        _isReadOnly = isReadOnly;

        foreach (var dataScope in DataDomainScope.DataPartitions.Values)
        {
            if (!dataScope.InterfaceType.IsAssignableFrom(typeof(T)))
                continue;

            //Filter.Add(e => (e as ICompanyEntry).CompanyNum == dataDomainScope.CurrentCompanyNum, FilterLayer.System);

            var param = Expression.Parameter(typeof(T), "e");

            var filterExpression = Expression.Lambda<Func<T, bool>>(
                Expression.Equal(
                    Expression.Property(
                        Expression.Convert(param, dataScope.InterfaceType),
                        dataScope.PartitionProperty
                    ),
                    Expression.Convert(Expression.Constant(dataScope.Value), dataScope.PartitionProperty.PropertyType)
                ),
                param
            );

            Filter.Add(filterExpression, FilterLayer.System);
        }
    }

    // TODO: replace the IsReadOnly to a ForUpdate or IsTrackingActivated option with the tracking happing right here in the DAO before it is passed to the repository
    public override bool IsReadOnly
    {
        get => _isReadOnly;
        protected set => _isReadOnly = value;
    }

    protected void TestIsNotReadonly([CallerMemberName] string? methodName = null)
    {
        if (IsReadOnly)
            throw new NotSupportedException(string.Format("This repository is loaded as read-only. Method {0} can not be used.", methodName));
    }

    public override void Init(T entry)
    {
        TestIsNotReadonly();
            
        base.Init(entry);

        foreach (var pair in DataDomainScope.DataPartitions)
        {
            var dataScope = pair.Value;

            if (!dataScope.InterfaceType.IsAssignableFrom(typeof(T)) ||
                // self protection, shall never happen
                dataScope.PartitionProperty.SetMethod == null)
                continue;

            dataScope.PartitionProperty.SetMethod.Invoke(
                entry,
                new[]
                {
                    Filter.GetDataPartitionValue(dataScope)
                }
            );
        }
    }

    private IQueryable<T> AddFilterToQueryable(IQueryable<T> queryable)
    {
        Expression<Func<T, bool>>? filterExpression = Filter.FilterExpression;
        if (filterExpression != null)
        {
            // Optimize expression tree for SQL query:
            filterExpression = (Expression<Func<T, bool>>?)ExpressionOptimizerVisitor.Singleton.Visit(filterExpression);

            if (filterExpression != null)
            {
                // add filter expression to query
                queryable = queryable.Where(filterExpression);
            }
        }

        return queryable;
    }

    public override IQueryable<T> AsQueryable()
    {
        if (Repository.IncludeNestedData != null)
        {
            Repository.IncludeNestedData.Clear();
            Repository.IncludeNestedData.AddRange(IncludeNestedData);
        }
        else if (IncludeNestedData.Count > 0)
            throw new NotSupportedException($"The repository does not support use of the property {nameof(IncludeNestedData)}");
        return AddFilterToQueryable(Repository.AsQueryable());
    }

    public override IQueryable<T> AsQueryableForUpdate()
    {
        return this.AsQueryable();

        //throw new NotSupportedException();
    }

    /// <summary>
    /// Returns a IQueryable&lt;T&gt; to get the nested
    /// objects of 
    /// </summary>
    /// <typeparam name="TNested"></typeparam>
    /// <param name="entity"></param>
    /// <param name="referenceName"></param>
    /// <returns></returns>
    public override IQueryable<TNested> GetNestedAsQueryable<TNested>(T entity, string? referenceName = null)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var references = Model.References
            .Where(r => r.RelationshipType == RelationshipType.ManyToOne &&
                        r.DependentEntityType == typeof(TNested) &&
                        (referenceName == null || r.Name == referenceName))
            .ToList();
        if (references.Count == 0)
        {
            var referenceExtension = referenceName == null ? "" : $" with name {referenceName}";
            throw new NotSupportedException($"Could not find ManyToOne reference for nested type {typeof(TNested).FullName}{referenceExtension}");
        }

        if (references.Count > 1)
        {
            if (referenceName == null)
                throw new NotSupportedException($"Found multiple ManyToMany references for nested type {typeof(TNested).FullName}. Use the {nameof(referenceName)} parameter to define which reference should be used.");

            throw new Exception("Inconsistency in entity model: Multiple references exist");
        }

        var nestedDao = DataDomainScope.GetDao<TNested>();

        var reference = references.First();

        if (reference.ForeignKeyProperties == null)
            throw new NotSupportedException(
                $"The reference {reference.Name} has no foreign key properties defined. Please check your definition again. You can't use this reference with the method {nameof(GetNestedAsQueryable)}");

        if (reference.PrincipalKeyProperties == null && nestedDao.Model.PrimaryKeyProperties == null)
            throw new NotSupportedException(
                $"The reference {reference.Name} has no principal key and primary key properties defined. Please check your definition again. You can't use this reference with the method {nameof(GetNestedAsQueryable)}");

        var foreignKeyProperties = reference.ForeignKeyProperties.ToList();
        var principalKeyProperties =
#pragma warning disable CS8604
            (reference.PrincipalKeyProperties ?? nestedDao.Model.PrimaryKeyProperties).ToList();
#pragma warning restore CS8604
            
        var nestedEntityParameter = Expression.Parameter(typeof(TNested), "nested");
        Expression? whereBodyExpression = null;

        for (int i = 0; i < foreignKeyProperties.Count; i++)
        {
            var principalKeyProperty = principalKeyProperties[i];
            var foreignKeyProperty = foreignKeyProperties[i];

            if (foreignKeyProperty.GetMethod == null)
                throw new Exception(
                    $"Foreign key Property {foreignKeyProperty.Name} of relationship {reference.Name} has no getter method. All properties used in a reference must have a public getter method.");

            var conditionExpression = Expression.Equal(
                Expression.Property(nestedEntityParameter, principalKeyProperty),
                Expression.Constant(
                    foreignKeyProperty.GetMethod.Invoke(entity, Array.Empty<object>()),
                    foreignKeyProperty.PropertyType
                )
            );

            whereBodyExpression = whereBodyExpression == null
                ? conditionExpression
                : Expression.And(whereBodyExpression, conditionExpression);
        }

        whereBodyExpression ??= Expression.Constant(true);

        Expression<Func<TNested, bool>> whereExpression = (Expression<Func<TNested, bool>>)Expression.Lambda(
            whereBodyExpression,
            nestedEntityParameter);

        return nestedDao.AsQueryable().Where(whereExpression);
    }

    #region Data partition handling

    // Data partition is the option to split the entity into different
    // sub-groups in the same relational table. This can be used e.g.
    // to split the entity objects by:
    // - Tenant
    // - Company

    private IEnumerable<IGrouping<object, T>> SplitIntoDataPartitions(ICollection<T> entries, Type interfaceType, PropertyInfo dataPartitionProperty)
    {
        var param = Expression.Parameter(typeof(T), "e");

        var groupByLambdaExpression = Expression.Lambda<Func<T, object>>(
            Expression.Property(
                Expression.Convert(param, interfaceType),
                dataPartitionProperty
            )
        );

        return entries.AsQueryable().GroupBy(groupByLambdaExpression);
    }

    protected async Task InvokePerDataPartitionAsync(ICollection<T> enumerable, Func<ICollection<T>, CancellationToken, Task> callback,
        CancellationToken cancellationToken, Index partitionIteration = default)
    {
        if (DataDomainScope.DataPartitions.Count < partitionIteration.Value)
        {
            var dataPartition = DataDomainScope.DataPartitions.ElementAt(partitionIteration).Value;

            if (dataPartition.InterfaceType.IsAssignableFrom(typeof(T)))
            {
                await Task.WhenAll(
                    SplitIntoDataPartitions(enumerable, dataPartition.InterfaceType, dataPartition.PartitionProperty)
                        .Select(g => InvokePerDataPartitionAsync(g.ToList(), callback, cancellationToken,
                            new Index(partitionIteration.Value + 1)))
                );
            }
            else
            {
                await InvokePerDataPartitionAsync(enumerable, callback, cancellationToken, new Index(partitionIteration.Value + 1));
            }
        }
        else
        {
            await callback(enumerable, cancellationToken);
        }
    }

    protected void InvokePerDataPartition(ICollection<T> enumerable, Func<ICollection<T>, CancellationToken, Task> callback,
        Index partitionIteration = default)
    {
        if (DataDomainScope.DataPartitions.Count < partitionIteration.Value)
        {
            var dataPartition = DataDomainScope.DataPartitions.ElementAt(partitionIteration).Value;

            if (dataPartition.InterfaceType.IsAssignableFrom(typeof(T)))
            {
                foreach (var groupedByDataPartition in SplitIntoDataPartitions(enumerable, dataPartition.InterfaceType, dataPartition.PartitionProperty))
                {
                    InvokePerDataPartition(groupedByDataPartition.ToList(), callback, new Index(partitionIteration.Value + 1));
                }
            }
            else
            {
                InvokePerDataPartition(enumerable, callback, new Index(partitionIteration.Value + 1));
            }
        }
        else
        {
            callback(enumerable, CancellationToken.None).Wait();
        }
    }

    #endregion

    #region CRUD operations

    public override void Create(T entity)
    {
        TestIsNotReadonly();
        if (IsOnCreateImplemented())
            OnCreate(new[] {entity}).Wait();
        Repository.Create(entity);
    }
        
    public override async Task CreateAsync(T entity)
    {
        TestIsNotReadonly();
        if (IsOnCreateImplemented())
            await OnCreate(new[] { entity });
        await Repository.CreateAsync(entity);
    }
        
    public override void Update(T entity)
    {
        TestIsNotReadonly();
        if (IsOnUpdateImplemented())
            OnUpdate(new[] {entity}).Wait();
        Repository.Update(entity, null);
    }

    public override async Task UpdateAsync(T entity)
    {
        TestIsNotReadonly();
        if (IsOnUpdateImplemented())
            await OnUpdate(new[] {entity});
        await Repository.UpdateAsync(entity, null);
    }

    public override void Delete(T entity)
    {
        TestIsNotReadonly();
        if (IsOnDeleteImplemented())
            OnDelete(new[] { entity }).Wait();
        Repository.Delete(entity);
    }
 
    public override async Task DeleteAsync(T entity)
    {
        TestIsNotReadonly();
        if (IsOnDeleteImplemented())
            await OnDelete(new[] { entity });
        await Repository.DeleteAsync(entity);
    }

    public override void CreateRange(IEnumerable<T> entries)
    {
        TestIsNotReadonly();
        if (IsOnCreateImplemented())
        {
            var enumerable = entries.ToList();
            InvokePerDataPartition(enumerable, OnCreate);
            Repository.CreateRange(enumerable);
        }
        else
        {
            Repository.CreateRange(entries);
        }
    }
        
    public override async Task CreateRangeAsync(IEnumerable<T> entries)
    {
        TestIsNotReadonly();
        if (IsOnCreateImplemented())
        {
            var enumerable = entries.ToList();
            await InvokePerDataPartitionAsync(enumerable, OnCreate, CancellationToken.None);
            await Repository.CreateRangeAsync(enumerable);
        }
        else
        {
            await Repository.CreateRangeAsync(entries);
        }
    }
        
    public override void DeleteRange(IEnumerable<T> entries)
    {
        TestIsNotReadonly();
        if (IsOnDeleteImplemented())
        {
            var enumerable = entries.ToList();
            InvokePerDataPartition(enumerable, OnDelete);
            Repository.DeleteRange(enumerable);
        }
        else
        {
            Repository.DeleteRange(entries);
        }
    }

    public override async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        TestIsNotReadonly();
        if (IsOnDeleteImplemented())
        {
            var enumerable = entities.ToList();
            await InvokePerDataPartitionAsync(enumerable, OnDelete, CancellationToken.None);
            await Repository.DeleteRangeAsync(enumerable);
        }
        else
        {
            await Repository.DeleteRangeAsync(entities);
        }
    }

    #endregion
}