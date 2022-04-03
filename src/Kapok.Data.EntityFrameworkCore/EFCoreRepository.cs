using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kapok.Core;
using Kapok.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kapok.Data.EntityFrameworkCore
{
    public class EFCoreRepository<T> : IRepository<T>
        where T : class, new()
    {
        private readonly EFCoreDataDomainScope _dataDomainScope;
        private readonly IEntityType _efCoreEntityType;

        public EFCoreRepository(EFCoreDataDomainScope dataDomainScope)
        {
            _dataDomainScope = dataDomainScope ?? throw new ArgumentNullException(nameof(dataDomainScope));

            var entityType = DbContext.Model.FindEntityType(typeof(T));
            _efCoreEntityType = entityType ?? throw new Exception($"Could not find model of entity type {typeof(T).FullName} in the DB Context (dataDomainScope.DbContext.Model.FindEntityType)");
        }

        private DbContext DbContext
        {
            get
            {
                var dbContext = _dataDomainScope.DbContext;

                if (dbContext == null)
                    throw new NotSupportedException(
                        "The IDataDomainScope connected to this repository is already disposed. You can not use this repository anymore.");

                return dbContext;
            }
        }

        protected DbSet<T> DbSet => DbContext.Set<T>();

        public ICollection<string>? IncludeNestedData { get; } = new List<string>();

        private IQueryable<T> AddNestedData(IQueryable<T> queryable)
        {
            if (IncludeNestedData == null)
                return queryable;

            // nested loading
            foreach (var navigationPropertyPath in IncludeNestedData)
            {
                queryable = queryable.Include(navigationPropertyPath);
            }

            return queryable;
        }

        public virtual IQueryable<T> AsQueryable()
        {
            IQueryable<T> queryable = DbSet.AsQueryable();

            // nested loading
            queryable = AddNestedData(queryable);

            return queryable.AsNoTracking();
        }

        protected virtual IReadOnlyList<IReadOnlyList<string>> GenerateIndexList()
        {
            var list = new List<IReadOnlyList<string>>();

            var primaryKey = _efCoreEntityType.FindPrimaryKey();
            if (primaryKey != null)
                list.Add(primaryKey.Properties.Select(p => p.Name).ToList());

            list.AddRange(_efCoreEntityType.GetIndexes().Select(
                i => i.Properties.Select(p => p.Name).ToList()
            ));

            return list;
        }

        // Cache the properties for an entity in the repository instance.
        // Note: we don't use a global cache here. This might be an option to get better performance.
        private PropertyInfo[]? _mapPropertiesWithKey;
        private PropertyInfo[]? _mapPropertiesWithoutKey;

        /// <summary>
        /// This method maps all properties from the <para>sourceInstance</para> to the <para>destinationInstance</para>
        /// so that Entity Framework Core recognizes which properties changed.
        ///
        /// We can not use a default map method here because we have to skip the navigation properties as well,
        /// otherwise EF core will recognize in some circumstances that an entity shall be deleted instead of
        /// just being modified.
        /// </summary>
        /// <param name="destinationInstance"></param>
        /// <param name="sourceInstance"></param>
        /// <param name="mapKey">
        /// If the primary key properties shall be mapped as well.
        /// </param>
        private void Map(T destinationInstance, T sourceInstance, bool mapKey = false)
        {
            PropertyInfo[]? iterateProperties;

            if (mapKey)
            {
                if (_mapPropertiesWithKey == null)
                {
                    var model = EntityBase.GetEntityModel<T>();
                    var keyProperties = (mapKey ? null : model.PrimaryKeyProperties) ?? Array.Empty<PropertyInfo>();

                    _mapPropertiesWithKey = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        // skip not mapped properties
                        .Where(p => !Attribute.IsDefined(p, typeof(NotMappedAttribute)))
                        // skip all primary keys
                        .Where(p => keyProperties.All(e2 => e2.Name != p.Name))
                        // skip all principal navigation properties
                        .Where(p => model.References.All(r => r.PrincipalNavigationProperty != p))
                        // skip properties where a getter or setter is not defined
                        .Where(p => p.GetMethod != null && p.SetMethod != null)
                        .ToArray();
                    
                }

                iterateProperties = _mapPropertiesWithKey;
            }
            else
            {
                if (_mapPropertiesWithoutKey == null)
                {
                    var model = EntityBase.GetEntityModel<T>();

                    _mapPropertiesWithoutKey = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        // skip all database generated properties
                        //.Where(p => ((DatabaseGeneratedAttribute?)Attribute.GetCustomAttribute(p, typeof(DatabaseGeneratedAttribute)))?.DatabaseGeneratedOption != DatabaseGeneratedOption.Identity)
                        // skip not mapped properties
                        .Where(p => !Attribute.IsDefined(p, typeof(NotMappedAttribute)))
                        // skip all principal navigation properties
                        .Where(p => model.References.All(r => r.PrincipalNavigationProperty != p))
                        // skip properties where a getter or setter is not defined
                        .Where(p => p.GetMethod != null && p.SetMethod != null)
                        .ToArray();
                }

                iterateProperties = _mapPropertiesWithoutKey;
            }

            // via reflection copy all values
            DtoMapper.Map(iterateProperties, sourceInstance, destinationInstance);
        }

        public virtual void Create(T entity) => DbSet.Add(entity);

        public virtual void Update(T entity, T? originalEntity)
        {
            var tempEntity = new T();

            lock (DbSet)
            {
                if (originalEntity != null)
                {
                    this.Map(tempEntity, originalEntity, mapKey: true);

                    // update changed properties only
                    DbSet.Attach(tempEntity);
                }

                this.Map(tempEntity , entity, mapKey: false);

                if (originalEntity == null)
                {
                    // update all properties
                    var trackingObject = DbSet.Update(tempEntity);

                    // .. except database generated properties
                    // NOTE: this property list cloud be cached to improve performance !
                    foreach (var trackingObjectProperty in trackingObject.Properties)
                    {
                        var propertyInfo = typeof(T).GetProperty(trackingObjectProperty.Metadata.Name);
                        var databaseGeneratedAttribute = propertyInfo?.GetCustomAttribute<DatabaseGeneratedAttribute>();

                        if (databaseGeneratedAttribute?.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
                        {
                            trackingObjectProperty.IsModified = false;
                        }
                    }
                }
            }
        }

        public virtual void Delete(T entity) => DbSet.Remove(entity);

        public virtual void CreateRange(IEnumerable<T> entities) => DbSet.AddRange(entities);

        public virtual void DeleteRange(IEnumerable<T> entities) => DbSet.RemoveRange(entities);

        public virtual async Task CreateAsync(T entity) => await DbSet.AddAsync(entity);

        public virtual Task UpdateAsync(T entity, T? originalEntity)
        {
            Update(entity, originalEntity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity)
        {
            DbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual async Task CreateRangeAsync(IEnumerable<T> entities) => await DbSet.AddRangeAsync(entities);

        public virtual Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            DbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }
    }
}
