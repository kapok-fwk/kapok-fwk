using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using Kapok.Data;
using Kapok.Entity;
using Res = Kapok.Resources.Data.DeferredDao;

namespace Kapok.BusinessLayer;

public static class DeferredDaoIQueryable
{
    public static IQueryable NotForUpdate<T>(this IQueryable source, IQueryable newSource)
        where T : class, new()
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (newSource is null)
            throw new ArgumentNullException(nameof(newSource));

        var genericVisitor = new QueryTranslatorExpressionVisitor(newSource, typeof(T));
        var expression = source.Expression;
        expression = genericVisitor.Visit(expression);
        return newSource.Provider.CreateQuery<T>(expression);
    }

    public static IQueryable<T> NotForUpdate<T>(this IQueryable<T> source)
        where T : class, new()
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (source.Provider is QueryTranslatorProvider<T> qtProvider)
        {
            var expression = source.Expression;

            expression = qtProvider.Visit(expression);

            return qtProvider.Source.Provider.CreateQuery<T>(expression);
        }

        // do nothing, is not of the right type
        return source;
    }
}

#pragma warning disable 693
// https://stackoverflow.com/questions/1839901/how-to-wrap-entity-framework-to-intercept-the-linq-expression-just-before-execut
internal class QueryTranslator<T> : IOrderedQueryable<T>
#pragma warning restore 693
{
    private readonly QueryTranslatorProvider<T> _provider;

    public QueryTranslator(IQueryable source, ChangeTracker changeTracker, PropertyInfo[] primaryKeyProperties, Dictionary<int, object>? primaryKeyIndex = null)
    {
        Expression = Expression.Constant(this);
        _provider = new QueryTranslatorProvider<T>(source, changeTracker, primaryKeyProperties, primaryKeyIndex);
    }

    public QueryTranslator(IQueryable source, Expression e, ChangeTracker changeTracker, PropertyInfo[] primaryKeyProperties, Dictionary<int, object>? primaryKeyIndex = null)
    {
        Expression = e ?? throw new ArgumentNullException(nameof(e));
        _provider = new QueryTranslatorProvider<T>(source, changeTracker, primaryKeyProperties, primaryKeyIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _provider.ExecuteEnumerableTyped(Expression).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _provider.ExecuteEnumerable(Expression).GetEnumerator();
    }

    public Type ElementType => typeof(T);

    public Expression Expression { get; }

    public IQueryProvider Provider => _provider;
}

#pragma warning disable 693
internal class QueryTranslatorProvider<T> : ExpressionVisitor, IQueryProvider
#pragma warning restore 693
{
    internal readonly IQueryable Source;
    internal readonly ChangeTracker ChangeTracker;
    private readonly PropertyInfo[] _primaryKeyProperties;
    private readonly Dictionary<int, object>? _primaryKeyIndex;

    public QueryTranslatorProvider(IQueryable source, ChangeTracker changeTracker, PropertyInfo[] primaryKeyProperties, Dictionary<int, object>? primaryKeyIndex = null)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        ChangeTracker = changeTracker;
        _primaryKeyProperties = primaryKeyProperties ?? throw new ArgumentNullException(nameof(primaryKeyProperties));
        _primaryKeyIndex = primaryKeyIndex;
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        if (expression == null) throw new ArgumentNullException(nameof(expression));

        return new QueryTranslator<TElement>(Source, expression, ChangeTracker, _primaryKeyProperties, _primaryKeyIndex);
    }

    public IQueryable CreateQuery(Expression expression)
    {
        if (expression == null) throw new ArgumentNullException(nameof(expression));
        Type elementType = expression.Type.GetGenericArguments().First();
#pragma warning disable CS8600
        IQueryable result = (IQueryable)Activator.CreateInstance(typeof(QueryTranslator<>).MakeGenericType(elementType),
            new object[] { Source, expression });
#pragma warning restore CS8600
#pragma warning disable CS8603
        return result;
#pragma warning restore CS8603
    }

    private IEnumerable ExecuteExtension(IEnumerable result)
    {
        foreach (var entity in result)
        {
            var currentEntity = entity;
            TrackCreateIfNotAlreadyTracked(ref currentEntity);
            yield return currentEntity;
        }
    }

    public TResult Execute<TResult>(Expression expression)
    {
        if (expression == null) throw new ArgumentNullException(nameof(expression));
#pragma warning disable CS8600
        object result = ((IQueryProvider)this).Execute(expression);
#pragma warning restore CS8600
#pragma warning disable CS8600
#pragma warning disable CS8603
        return (TResult)result;
#pragma warning restore CS8603
#pragma warning restore CS8600
    }

    public object Execute(Expression expression)
    {
        if (expression == null) throw new ArgumentNullException(nameof(expression));

        Expression translated = this.Visit(expression);
        var entity = Source.Provider.Execute(translated);

        if (entity != null)
            TrackCreateIfNotAlreadyTracked(ref entity);

#pragma warning disable CS8603
        return entity;
#pragma warning restore CS8603
    }

    internal IEnumerable<T> ExecuteEnumerableTyped(Expression expression)
    {
        if (expression == null) throw new ArgumentNullException(nameof(expression));

        Expression translated = this.Visit(expression);
        var result = (IEnumerable<T>)Source.Provider.CreateQuery(translated);

        foreach (var entity in result)
        {
            object? currentEntity = entity;
#pragma warning disable CS8601
            TrackCreateIfNotAlreadyTracked(ref currentEntity);
#pragma warning restore CS8601
            yield return (T)currentEntity;
        }
    }
    internal IEnumerable ExecuteEnumerable(Expression expression)
    {
        if (expression == null) throw new ArgumentNullException(nameof(expression));

        Expression translated = this.Visit(expression);
        var result = Source.Provider.CreateQuery(translated);

        return ExecuteExtension(result);
    }

    private static void Map(object oldEntry, object newEntry, Type entityType)
    {
        var type = oldEntry.GetType();

        var model = EntityBase.GetEntityModel(entityType);
        var properties = model.PrimaryKeyProperties;

        // via reflection copy all values
        foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     // skip all primary keys
                     .Where(e => properties?.Any(e2 => e2.Name != e.Name) ?? true))
        {
            // ignore all fields which are not mapped on data-level
            if (Attribute.IsDefined(propertyInfo, typeof(NotMappedAttribute)))
                continue;

            if (propertyInfo.GetMethod == null || propertyInfo.SetMethod == null)
                continue;

            propertyInfo.SetMethod.Invoke(oldEntry, new[]
            {
                propertyInfo.GetMethod.Invoke(newEntry, null)
            });
        }
    }

    internal void TrackCreateIfNotAlreadyTracked(ref object entity)
    {
        var entityPkHash = entity.GetPrimaryKeyHash(_primaryKeyProperties);
        if (_primaryKeyIndex != null)
        {
            if (_primaryKeyIndex.ContainsKey(entityPkHash))
            {
                if (!ReferenceEquals(_primaryKeyIndex[entityPkHash], entity))
                {
                    var oldTrackedEntity = _primaryKeyIndex[entityPkHash];
                    var oldTrackObject = ChangeTracker.Get(oldTrackedEntity);
                    if (oldTrackObject != null)
                    {
                        ChangeTracker.Detach(oldTrackObject);
                    }
                    Map(oldTrackedEntity, entity, typeof(T));
                    entity = oldTrackedEntity;
                }
            }
            else
            {
                _primaryKeyIndex.Add(entityPkHash, entity);
            }
        }

        var trackingObject = ChangeTracker.Get(entity);
        if (trackingObject == null)
        {
            ChangeTracker.Add(entity);
        }
        else if (trackingObject.OriginalEntity == null)
        {
            trackingObject.OriginalEntity = entity;
        }
    }

    #region Visitors
    protected override Expression VisitConstant(ConstantExpression c)
    {
        // fix up the Expression tree to work with EF again
        if (c.Type == typeof(QueryTranslator<T>))
        {
            return Source.Expression;
        }
        else
        {
            return base.VisitConstant(c);
        }
    }
    #endregion
}

internal class QueryTranslatorExpressionVisitor : ExpressionVisitor
{
    private readonly IQueryable _source;
    private readonly Type _entityType;

    public QueryTranslatorExpressionVisitor(IQueryable source, Type entityType)
    {
        _source = source;
        _entityType = entityType;
    }

    #region Visitors
    protected override Expression VisitConstant(ConstantExpression c)
    {
        // fix up the Expression tree to work with EF again
        if (c.Type.IsGenericType &&
            c.Type.GetGenericTypeDefinition() == typeof(QueryTranslator<>) &&
            c.Type.GenericTypeArguments[0] == _entityType)
        {
            return _source.Expression;
        }
        else
        {
            return base.VisitConstant(c);
        }
    }
    #endregion
}

public class DeferredDao<T> : Dao<T>, IDeferredCommitDao
    where T : class, new()
{
    private readonly ChangeTracker _changeTracker = new();

    /// <summary>
    /// A list of all properties of the <c>T</c> entity which are part of
    /// the primary key. If there exist no primary key, the field
    /// is <c>null</c>.
    /// </summary>
    private readonly PropertyInfo[] _primaryKeyProperties;

    // key = hash of the primary key fields
    // value = entity object
    private readonly Dictionary<int, object> _primaryKeyIndex = new();

    public DeferredDao(IDataDomainScope dataDomainScope, IRepository<T> repository)
        : base(dataDomainScope, repository)
    {
        var primaryKeys = EntityBase.GetEntityModel<T>().PrimaryKeyProperties;

        if (primaryKeys == null)
            throw new NotSupportedException(
                $"You cannot use {nameof(DeferredDao<T>)} with a Type T which does not have a primary key. T = {typeof(T).FullName}");

        _primaryKeyProperties = primaryKeys;
    }

    public DeferredDao(IDataDomainScope dataDomainScope) : base(dataDomainScope)
    {
        var primaryKeys = EntityBase.GetEntityModel<T>().PrimaryKeyProperties;

        if (primaryKeys == null)
            throw new NotSupportedException(
                $"You cannot use {nameof(DeferredDao<T>)} with a Type T which does not have a primary key. T = {typeof(T).FullName}");

        _primaryKeyProperties = primaryKeys;
    }

    public override IQueryable<T> AsQueryableForUpdate()
    {
        if (_primaryKeyProperties == null)
            throw new NotSupportedException(
                string.Format(Res.EntityTypeHasNoPrimaryKey,
                    nameof(AsQueryableForUpdate), typeof(T).FullName));

        return new QueryTranslator<T>(base.AsQueryable(), _changeTracker, _primaryKeyProperties, _primaryKeyIndex);
    }

    private void RegisterEvent(object entity)
    {
        if (entity is INotifyPropertyChanged entityNotifyPropertyChanged)
            entityNotifyPropertyChanged.PropertyChanged += EntityPropertyChanged;
    }

    private void UnregisterEvent(object entity)
    {
        if (entity is INotifyPropertyChanged entityNotifyPropertyChanged)
            entityNotifyPropertyChanged.PropertyChanged -= EntityPropertyChanged;
    }

    private void TrackCreated(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var entityPkHash = entity.GetPrimaryKeyHash(_primaryKeyProperties);
        if (_primaryKeyIndex.ContainsKey(entityPkHash))
        {
            foreach (var primaryKeyProperty in _primaryKeyProperties)
            {
                var databaseGenerated = primaryKeyProperty.GetCustomAttribute<DatabaseGeneratedAttribute>();
                if (databaseGenerated?.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
                {
                    entityPkHash = 0;
                    break;
                }
            }

            if (entityPkHash != 0)
                throw new NotSupportedException(
                    string.Format(Res.CreateExistingEntityError,
                        typeof(T).FullName, entity.GetPrimaryKeyAsString(_primaryKeyProperties)));
        }

        var trackingObject = _changeTracker.Add(entity);
        trackingObject.State = ChangeTrackingState.Created;
        if (entityPkHash != 0)
        {
            _primaryKeyIndex.Add(entityPkHash, entity);
            RegisterEvent(entity);
        }
    }
    private void TrackUpdate(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var entityPkHash = entity.GetPrimaryKeyHash(_primaryKeyProperties);
        if (_primaryKeyIndex.ContainsKey(entityPkHash) &&
            !ReferenceEquals(_primaryKeyIndex[entityPkHash], entity))
        {
            var oldTrackedEntity = _primaryKeyIndex[entityPkHash];
            oldTrackedEntity.Map(entity);
            entity = (T)oldTrackedEntity;
        }

        var trackingObject = _changeTracker.Get(entity);
        if (trackingObject == null)
            throw new NotSupportedException(
                string.Format(Res.UpdateNotTrackedEntityError,
                    typeof(T).FullName, nameof(StartChangeTracking)));

        if (trackingObject.State == ChangeTrackingState.Created)
        {
            return;
        }

        if (trackingObject.OriginalEntity == null)
        {
            // This should never happen, but just in case ...
            throw new NotSupportedException(
                string.Format(Res.TrackUpdateInconsistencyInChangeTrackerObject,
                    nameof(trackingObject.OriginalEntity), nameof(TrackUpdate), typeof(T).FullName, entity.GetPrimaryKeyAsString(_primaryKeyProperties)));
        }
            
        trackingObject.State = ChangeTrackingState.Updated;
    }
    private void TrackDelete(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var entityPkHash = entity.GetPrimaryKeyHash(_primaryKeyProperties);
        if (_primaryKeyIndex.ContainsKey(entityPkHash) &&
            !ReferenceEquals(_primaryKeyIndex[entityPkHash], entity))
        {
            var oldTrackedEntity = _primaryKeyIndex[entityPkHash];
            oldTrackedEntity.Map(entity);
            entity = (T)oldTrackedEntity;
        }

        var trackingObject = _changeTracker.Get(entity);
        if (trackingObject == null)
        {
            trackingObject = _changeTracker.Add(entity);
            trackingObject.OriginalEntity = null;
            _primaryKeyIndex.Add(entityPkHash, entity);
            RegisterEvent(entity);
        }
        else if (trackingObject.State == ChangeTrackingState.Created)
        {
            _changeTracker.Detach(trackingObject);
            _primaryKeyIndex.Remove(entityPkHash);
            UnregisterEvent(entity);
            return;
        }
        trackingObject.State = ChangeTrackingState.Deleted;
    }

    public override void Create(T entity)
    {
        TestIsNotReadonly();
        if (IsOnCreateImplemented())
            OnCreate(new[] { entity }).Wait();

        TrackCreated(entity);
    }
    public override void Update(T entity)
    {
        TestIsNotReadonly();
        if (IsOnUpdateImplemented())
            OnUpdate(new[] { entity }).Wait();

        TrackUpdate(entity);
    }
    public override void Delete(T entity)
    {
        TestIsNotReadonly();
        if (IsOnDeleteImplemented())
            OnDelete(new[] { entity }).Wait();

        TrackDelete(entity);
    }

    public override async Task CreateAsync(T entity)
    {
        TestIsNotReadonly();
        if (IsOnCreateImplemented())
            await OnCreate(new[] { entity });
            
        TrackCreated(entity);
    }
    public override async Task UpdateAsync(T entity)
    {
        TestIsNotReadonly();
        if (IsOnUpdateImplemented())
            await OnUpdate(new[] { entity });
            
        TrackUpdate(entity);
    }
    public override async Task DeleteAsync(T entity)
    {
        TestIsNotReadonly();
        if (IsOnDeleteImplemented())
            await OnDelete(new[] { entity });
            
        TrackDelete(entity);
    }

    public override void CreateRange(IEnumerable<T> entities)
    {
        TestIsNotReadonly();
        if (IsOnCreateImplemented())
        {
            var enumerable = entities.ToList();
            InvokePerDataPartition(enumerable, OnCreate);
            enumerable.ForEach(Create);
        }
        else
        {
            foreach (var entity in entities)
            {
                Create(entity);
            }
        }
    }
    public override void DeleteRange(IEnumerable<T> entities)
    {
        TestIsNotReadonly();
        if (IsOnDeleteImplemented())
        {
            var enumerable = entities.ToList();
            InvokePerDataPartition(enumerable, OnDelete);
            enumerable.ForEach(Delete);
        }
        else
        {
            foreach (var entity in entities)
            {
                Delete(entity);
            }
        }
    }

    public override async Task CreateRangeAsync(IEnumerable<T> entities)
    {
        TestIsNotReadonly();
        if (IsOnCreateImplemented())
        {
            var enumerable = entities.ToList();
            await InvokePerDataPartitionAsync(enumerable, OnCreate, CancellationToken.None);
            CreateRange(enumerable);
        }
        else
        {
            CreateRange(entities);
        }
    }
    public override async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        TestIsNotReadonly();
        if (IsOnDeleteImplemented())
        {
            var enumerable = entities.ToList();
            await InvokePerDataPartitionAsync(enumerable, OnDelete, CancellationToken.None);
            DeleteRange(enumerable);
        }
        else
        {
            DeleteRange(entities);
        }
    }

    #region IDeferredCommitDao

    public bool CanSave()
    {
        return _changeTracker.AnyChangesOutstanding();
    }

    public void Save()
    {
        _lastChangedObjects = _changeTracker.Where(
            to => to.State == ChangeTrackingState.Created ||
                  to.State == ChangeTrackingState.Updated ||
                  to.State == ChangeTrackingState.Deleted
        ).ToList();

        foreach (var trackingObject in _lastChangedObjects)
        {
            switch (trackingObject.State)
            {
                case ChangeTrackingState.Created:
                    Repository.Create((T) trackingObject.Entity);
                    trackingObject.State = ChangeTrackingState.None;
                    trackingObject.OriginalEntity = null; // Note: mapping is done in PostSave() because then we include the database generated values e.g. auto-increment and row version column
                    break;
                case ChangeTrackingState.Updated:
                    Repository.Update((T)trackingObject.Entity, (T?)trackingObject.OriginalEntity);
                    trackingObject.State = ChangeTrackingState.None;
                    trackingObject.OriginalEntity = DtoMapper.Map(trackingObject.EntityType, trackingObject.Entity);
                    break;
                case ChangeTrackingState.Deleted:
                    Repository.Delete((T) (trackingObject.OriginalEntity ?? trackingObject.Entity));
                    trackingObject.State = ChangeTrackingState.Detached;
                    trackingObject.OriginalEntity = null;
                    break;
            }
        }
    }

    private List<ChangeTracking>? _lastChangedObjects;

    public void RejectChanges()
    {
        /*foreach (var trackingObject in _changeTracker
            .Where(to => to.State == ChangeTrackingState.Created || to.State == ChangeTrackingState.Updated || to.State == ChangeTrackingState.Deleted)
            .ToList())
        {
            switch (trackingObject.State)
            {
                case ChangeTrackingState.Created:
                    _changeTracker.Detach(trackingObject);
                    break;
                case ChangeTrackingState.Updated:
                    throw new NotImplementedException();
                case ChangeTrackingState.Deleted:
                    if (trackingObject.OriginalEntity == null)
                    {
                        _changeTracker.Detach(trackingObject);
                    }
                    else
                    {
                        trackingObject.State = ChangeTrackingState.None;
                    }
                    break;
            }
        }*/

        // TODO this implementation is wrong! we should here map the entities back to their original state or deprecate this function!
        _changeTracker.Clear();
        _primaryKeyIndex.Clear();
    }

    public void PostSave()
    {
        if (_primaryKeyIndex.Count > 0)
            _primaryKeyIndex.Clear();

        if (_lastChangedObjects == null || _lastChangedObjects.Count == 0)
            return;

        var rowVersionProperty = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.CanRead && p.GetCustomAttribute<TimestampAttribute>() != null);

        IDataDomainScope? scope = null;
        IDao<T>? scopeDao = null;

        try
        {
            foreach (var trackingObject in _lastChangedObjects.Where(to => to.State != ChangeTrackingState.Detached).ToList())
            {
                if (_primaryKeyProperties != null)
                {
                    var pkHash = trackingObject.Entity.GetPrimaryKeyHash(_primaryKeyProperties);
                    _primaryKeyIndex.Add(pkHash, (T)trackingObject.Entity);
                }

                if (trackingObject.State == ChangeTrackingState.None && trackingObject.OriginalEntity == null)
                {
                    trackingObject.OriginalEntity = DtoMapper.Map(trackingObject.EntityType, trackingObject.Entity);
                }

                // make sure that we update the row version column so that we can call a second "update" on an entry
                if (rowVersionProperty != null)
                {
                    // TODO we must initialize here a separate scope because EF Core seems to cache the last version! We need to find a way to work around this issue!
                    if (scope == null)
                    {
                        scope = DataDomainScope.DataDomain.CreateScope();
                        scopeDao = scope.GetDao<T>();
                    }

                    var entity = trackingObject.Entity;

                    lock (entity)
                    {
                        // TODO: here we get the whole item but we should change it so that we just get the scalar value "RowVersion" to reduce db traffic!
#pragma warning disable CS8604
                        var newEntry = scopeDao.FindByKey(entity.GetPrimaryKeyValues());
#pragma warning restore CS8604

                        if (newEntry != null)
                        {
                            UnregisterEvent(entity);
                            _changeTracker.Detach(trackingObject);

#pragma warning disable CS8602
                            rowVersionProperty.SetMethod.Invoke(
                                entity,
                                new[]
                                {
                                    rowVersionProperty.GetMethod.Invoke(newEntry, null)
                                }
                            );
#pragma warning restore CS8602

                            _changeTracker.Add(entity);
                            RegisterEvent(entity);
                        }
                    }
                }
            }
        }
        finally
        {
            scope?.Dispose();
        }
    }

    /// <summary>
    /// Start tracking if the entity is not already tracked.
    ///
    /// If the entity is already tracked, nothing happens.
    /// </summary>
    /// <param name="entity"></param>
    public void StartChangeTracking(object entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var entityPkHash = entity.GetPrimaryKeyHash(_primaryKeyProperties);
        ChangeTrackingState? newChangeTrackingState = null;
        if (_primaryKeyIndex.ContainsKey(entityPkHash) &&
            !ReferenceEquals(_primaryKeyIndex[entityPkHash], entity))
        {
            var oldTrackedEntity = _primaryKeyIndex[entityPkHash];
            var oldTrackingObject = _changeTracker.Get(oldTrackedEntity);
            if (oldTrackingObject != null)
            {
                switch (oldTrackingObject.State)
                {
                    case ChangeTrackingState.Created:
                        throw new NotSupportedException(
                            string.Format(Res.StartTrackingAlreadyTrackendEntryStateCreated, typeof(T).FullName));
                    case ChangeTrackingState.Updated:
                            
                        // since we marked a row here for update, we override the entity properties

                        foreach (var propertyInfo in _primaryKeyProperties.Where(p => p.CanRead && p.CanWrite))
                        {
                            var timestampAttribute = propertyInfo.GetCustomAttribute<TimestampAttribute>();
                            if (timestampAttribute != null)
                            {
                                // NOTE We do here a check if the 'entity' version is another version
                                //      as the already tracked version and if that is the case throw an exception.
                                //
                                //      This is pessimistic behavior. This is a pre-catch of possible concurrency check exceptions
                                //      and indicates possible bad application design.

#pragma warning disable CS8602 // Dereference of a possibly null reference
                                var oldRowVersion = propertyInfo.GetMethod.Invoke(oldTrackedEntity, Array.Empty<object>());
#pragma warning restore CS8602 // Dereference of a possibly null reference
                                var newRowVersion = propertyInfo.GetMethod.Invoke(entity, Array.Empty<object>());

                                if (oldRowVersion != newRowVersion)
                                {
                                    throw new NotSupportedException(
                                        string.Format(Res.StartTrackingAlreadyTrackendEntryStateUpdated,
                                            typeof(T).FullName, nameof(IDataDomainScope), nameof(StartChangeTracking)));
                                }

                                continue;
                            }

                            // move all data changed in the updated entity to the new instance
#pragma warning disable CS8602 // Dereference of a possibly null reference
                            propertyInfo.SetMethod.Invoke(entity, new[]
                            {
                                propertyInfo.GetMethod.Invoke(oldTrackedEntity, Array.Empty<object>())
                            });
#pragma warning restore CS8602 // Dereference of a possibly null reference
                        }

                        break;
                    case ChangeTrackingState.Deleted:
                        newChangeTrackingState = ChangeTrackingState.Deleted;
                        break;
                    case ChangeTrackingState.None:
                    case ChangeTrackingState.Detached:
                        // no special treatment
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _changeTracker.Detach(oldTrackingObject);
                UnregisterEvent(oldTrackingObject);
            }
            else
            {
#if DEBUG
                    throw new NotSupportedException(
                        string.Format(Res.PrimaryKeyCacheInconsistencyError,
                            typeof(T).FullName, ((T)entity).GetPrimaryKeyAsString(_primaryKeyProperties), nameof(StartChangeTracking)));
#else
                // Using optimistic behavior --> delete the wrong primary key cache entry and continue

                // TODO implement logging here
#endif
            }

            _primaryKeyIndex.Remove(entityPkHash);
        }

        var trackingObject = _changeTracker.Get(entity);

        if (trackingObject == null)
        {
            trackingObject = _changeTracker.Add(entity);
            if (entityPkHash != 0)
                _primaryKeyIndex.Add(entityPkHash, (T)entity);
            RegisterEvent(entity);
        }

        if (newChangeTrackingState.HasValue)
            trackingObject.State = newChangeTrackingState.Value;

    }

    /// <summary>
    /// we have to update the primary key index hash when the property index have been changed...
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    private void EntityPropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (sender == null || eventArgs.PropertyName == null)
            return;

        var entity = (T)sender;

        var primaryKeyField = _primaryKeyProperties.FirstOrDefault(pkf => pkf.Name == eventArgs.PropertyName);
        if (primaryKeyField != null)
        {
            var pair = _primaryKeyIndex.FirstOrDefault(pair => pair.Value == sender);

            var newHash = sender.GetPrimaryKeyHash(_primaryKeyProperties);

            if (pair.Key != newHash)
            {
                // the primary key changed here! we update the has at this code point.
                _primaryKeyIndex.Remove(pair.Key);
                _primaryKeyIndex.Add(newHash, entity);
            }
        }
    }

    #endregion
}