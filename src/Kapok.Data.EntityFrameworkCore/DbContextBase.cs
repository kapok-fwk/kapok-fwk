#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Kapok.Entity;
using Kapok.Entity.Model;
using Newtonsoft.Json.Linq;
using PrecisionAttribute = Kapok.Entity.PrecisionAttribute;
using DeleteBehavior = Kapok.Entity.Model.DeleteBehavior;
using ModelBuilder = Microsoft.EntityFrameworkCore.ModelBuilder;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace Kapok.Data.EntityFrameworkCore;

public class DbContextBase : DbContext
{
    private struct AutoGeneratePropertyCache
    {
        public Type EntityType;
        public PropertyInfo PropertyInfo;
        public AutoGenerateValueType AutoGenerateType;

        public void SetPropertyValue(object entityType, object value)
        {
            if (PropertyInfo == null)
                throw new Exception($"Member {PropertyInfo} is not set");

            if (PropertyInfo.SetMethod == null)
                throw new Exception($"The property {PropertyInfo} has no setter method");

            PropertyInfo.SetMethod.Invoke(entityType, new[] {value});
        }
    }

    private static readonly List<AutoGeneratePropertyCache> EntityAutoGenerateProperties = new();

    public DbContextBase(DbContextOptions options)
        : base(options)
    {
    }

    protected DbContextBase()
    {
    }

#if DEBUG
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .EnableSensitiveDataLogging();
    }
#endif
    
    public const string SqliteProvider = "Microsoft.EntityFrameworkCore.Sqlite";
    public const string PostgreSqlProvider = "Npgsql.EntityFrameworkCore.PostgreSQL";
    public const string SqlServerProvider = "Microsoft.EntityFrameworkCore.SqlServer";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // default relationship delete behavior is always restrict
        // @TODO: check if we should use this
        // source: https://stackoverflow.com/questions/34768976/specifying-on-delete-no-action-in-entity-framework-7
        /*foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }*/

        base.OnModelCreating(modelBuilder);

        // build models
        foreach (var entityType in DataDomain.DataEntities)
        {
            // call modelBuilder.Entity<TEntity>()
            var method = (from m in typeof(ModelBuilder).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where m.Name == nameof(ModelBuilder.Entity) &&
                      m.IsGenericMethodDefinition
                select m).FirstOrDefault();
            // ReSharper disable once PossibleNullReferenceException
            EntityTypeBuilder? entityTypeBuilder = (EntityTypeBuilder?)method
                ?.MakeGenericMethod(entityType)
                .Invoke(modelBuilder, null);
            if (entityTypeBuilder == null)
                throw new NotSupportedException("The method modelBuilder.Entity<TEntity>() must return a value and cannot be null.");

            var entityModel = EntityBase.GetEntityModel(entityType);

            var sqlViewEntityAttribute = entityType.GetCustomAttribute<SqlViewEntityAttribute>();
            if (sqlViewEntityAttribute != null)
            {
                var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();

                string tableName = tableAttribute?.Name ?? entityType.Name;
                string? schemaName = tableAttribute?.Schema;

                // Note: Since EF Core 5.0 we have to use here ToTable instead of ToView
                //       to exclude the view from the migrations. See as well:
                //       https://docs.microsoft.com/de-de/ef/core/what-is-new/ef-core-5.0/breaking-changes#toview-is-treated-differently-by-migrations

                if (schemaName != null)
                {
                    entityTypeBuilder
                        .ToTable(tableName, schemaName, t => t.ExcludeFromMigrations());
                }
                else
                {
                    entityTypeBuilder
                        .ToTable(tableName, t => t.ExcludeFromMigrations());
                }
            }

            if (entityModel.PrimaryKeyProperties != null)
            {
                entityTypeBuilder
                    .HasKey(entityModel.PrimaryKeyProperties.Select(p => p.Name).ToArray())
                    .HasName($"PK_{entityType.Name}");
            }
            else
            {
                entityTypeBuilder
                    .HasNoKey();
            }

            foreach (var index in entityModel.Indexes)
            {
                // NOTE: we don't add an index name here
                entityTypeBuilder
                    .HasIndex(index.Properties.Select(p => p.Name).ToArray())
                    .IsUnique(index.IsUnique);
            }

            if (sqlViewEntityAttribute == null)
            {
                foreach (var reference in entityModel.References
                             .Where(r => r.PrincipalEntityType == entityType &&
                                         (r.RelationshipType == RelationshipType.OneToOne ||
                                          r.RelationshipType == RelationshipType.OneToMany))
                        )
                {
                    switch (reference.RelationshipType)
                    {
                        case RelationshipType.OneToOne:
                        {
                            var refBuilder = 
                                reference.PrincipalNavigationProperty == null
                                    ? entityTypeBuilder.HasOne(reference.DependentEntityType)
                                    : entityTypeBuilder.HasOne(reference.DependentEntityType, reference.PrincipalNavigationProperty.Name);

                            var refBuilder2 = reference.ForeignNavigationProperty == null
                                ? refBuilder.WithOne()
                                : refBuilder.WithOne(reference.ForeignNavigationProperty.Name);

                            if (reference.PrincipalKeyProperties != null)
                                refBuilder2 = refBuilder2.HasPrincipalKey(reference.PrincipalEntityType,
                                    reference.PrincipalKeyProperties.Select(p => p.Name).ToArray());
                            if (reference.ForeignKeyProperties != null)
                                refBuilder2 = refBuilder2.HasForeignKey(reference.DependentEntityType,
                                    reference.ForeignKeyProperties.Select(p => p.Name).ToArray());

                            switch (reference.DeleteBehavior)
                            {
                                case DeleteBehavior.NoAction:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.NoAction);
                                    break;
                                case DeleteBehavior.SetNull:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior
                                        .ClientSetNull);
                                    break;
                                case DeleteBehavior.Restrict:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
                                    break;
                                case DeleteBehavior.Cascade:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior
                                        .ClientCascade);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                            break;
                        case RelationshipType.OneToMany:
                        {
                            var refBuilder = 
                                reference.PrincipalNavigationProperty == null
                                    ? entityTypeBuilder.HasOne(reference.DependentEntityType)
                                    : entityTypeBuilder.HasOne(reference.DependentEntityType, reference.PrincipalNavigationProperty.Name);

                            var refBuilder2 = reference.ForeignNavigationProperty == null
                                ? refBuilder.WithMany()
                                : refBuilder.WithMany(reference.ForeignNavigationProperty.Name);

                            if (reference.PrincipalKeyProperties != null)
                                refBuilder2 = refBuilder2.HasPrincipalKey(reference.PrincipalKeyProperties
                                    .Select(p => p.Name).ToArray());
                            if (reference.ForeignKeyProperties != null)
                                refBuilder2 = refBuilder2.HasForeignKey(reference.ForeignKeyProperties
                                    .Select(p => p.Name).ToArray());

                            switch (reference.DeleteBehavior)
                            {
                                case DeleteBehavior.NoAction:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.NoAction);
                                    break;
                                case DeleteBehavior.SetNull:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior
                                        .ClientSetNull);
                                    break;
                                case DeleteBehavior.Restrict:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
                                    break;
                                case DeleteBehavior.Cascade:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior
                                        .ClientCascade);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                            break;
                        case RelationshipType.ManyToOne:
                        {
                            var refBuilder = 
                                reference.PrincipalNavigationProperty == null
                                    ? entityTypeBuilder.HasMany(reference.DependentEntityType)
                                    : entityTypeBuilder.HasMany(reference.DependentEntityType, reference.PrincipalNavigationProperty.Name);

                            var refBuilder2 = reference.ForeignNavigationProperty == null
                                ? refBuilder.WithOne()
                                : refBuilder.WithOne(reference.ForeignNavigationProperty.Name);

                            if (reference.PrincipalKeyProperties != null)
                                refBuilder2 = refBuilder2.HasPrincipalKey(reference.PrincipalKeyProperties
                                    .Select(p => p.Name).ToArray());
                            if (reference.ForeignKeyProperties != null)
                                refBuilder2 = refBuilder2.HasForeignKey(reference.ForeignKeyProperties
                                    .Select(p => p.Name).ToArray());

                            switch (reference.DeleteBehavior)
                            {
                                case DeleteBehavior.NoAction:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.NoAction);
                                    break;
                                case DeleteBehavior.SetNull:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior
                                        .ClientSetNull);
                                    break;
                                case DeleteBehavior.Restrict:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
                                    break;
                                case DeleteBehavior.Cascade:
                                    refBuilder2.OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior
                                        .ClientCascade);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            foreach (var property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                if (Attribute.IsDefined(property, typeof(NotMappedAttribute)))
                    continue;

                if (property.PropertyType == typeof(JsonObject) ||
                    property.PropertyType == typeof(JsonArray) ||
#if USE_JSON_LIBRARY_NEWTONSOFT
                    property.PropertyType == typeof(JObject) ||
                    property.PropertyType == typeof(JArray) ||
#endif
                    property.PropertyType == typeof(Caption))
                {
                    entityTypeBuilder.Property(property.Name)
                        .HasJsonValueConversion(property.PropertyType);
                }
                else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                {
                    var precisionAttr = property.GetCustomAttribute<PrecisionAttribute>();

                    int precision = precisionAttr?.Precision ?? 38;
                    int scale = precisionAttr?.Scale ?? 0;

                    entityTypeBuilder.Property(property.Name).HasPrecision(precision, scale);
                }
                else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                {
                    var dataTypeAttr = property.GetCustomAttribute<DataTypeAttribute>();

                    if (dataTypeAttr != null)
                    {
                        switch (dataTypeAttr.DataType)
                        {
                            case DataType.Date:
                                entityTypeBuilder.Property(property.Name).HasColumnType("date");
                                break;
                            //case DataType.Time:
                            //    entityTypeBuilder.Property(property.Name).HasColumnType("time(0)");
                            //    break;
                            //case DataType.DateTime:
                            //    entityTypeBuilder.Property(property.Name).HasColumnType("datetimeoffset");
                            //    break;
                        }
                    }    
                }

                var autoGenerateValueAttribute = property.GetCustomAttribute<AutoGenerateValueAttribute>();
                if (autoGenerateValueAttribute != null)
                {
                    // NOTE: maybe add here a warning / error when an auto generated property type is used several times
                    //       in an entity (which does e.g. not make sense with the CreatedDateTime type).
                    EntityAutoGenerateProperties.Add(new AutoGeneratePropertyCache
                    {
                        EntityType = entityType,
                        PropertyInfo = property,
                        AutoGenerateType = autoGenerateValueAttribute.Type
                    });

                    switch (autoGenerateValueAttribute.Type)
                    {
                        case AutoGenerateValueType.CreatedDateTime:
                        case AutoGenerateValueType.LastModifiedDateTime:
                            if (property.PropertyType != typeof(DateTime))
                            {
                                throw new NotSupportedException(
                                    $"The auto generate data property type {autoGenerateValueAttribute.Type} can only be assigned to properties of the type {typeof(DateTime).Name}.");
                            }

                            entityTypeBuilder.Property(property.Name)
                                .HasDefaultValueSql(SqlFunctionCurrentDateTimeUtc(Database?.ProviderName));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }

    private static string SqlFunctionCurrentDateTimeUtc(string? providerName)
    {
        if (providerName == null)
            throw new ArgumentNullException(nameof(providerName));

        if (providerName == SqliteProvider)
        {
            return "DATETIME('now')";
        }

        if (providerName == PostgreSqlProvider)
        {
            return "timezone('utc', now())";
        }

        if (providerName == SqlServerProvider)
        {
            return "GetUtcDate()";
        }

        throw new NotSupportedException(
            $"Function {nameof(SqlFunctionCurrentDateTimeUtc)} is not supported for provider '{providerName}'");
    }

    public override int SaveChanges()
    {
        var now = DateTime.UtcNow;

        foreach (var changedEntry in ChangeTracker.Entries()
                     .Where(e => e.State != EntityState.Unchanged))
        {
            var entityType = changedEntry.Entity.GetType();

            foreach (var field in from f in EntityAutoGenerateProperties
                     where f.EntityType == entityType
                     select f)
            {
                switch (field.AutoGenerateType)
                {
                    case AutoGenerateValueType.CreatedDateTime:
                        switch (changedEntry.State)
                        {
                            case EntityState.Added:
                                // set initial value for the created date time property
                                field.SetPropertyValue(changedEntry.Entity, now);
                                break;
                            case EntityState.Modified:
                                // ignore all changes to an created date time property
                                if (changedEntry.Property(field.PropertyInfo.Name).IsModified)
                                    changedEntry.Property(field.PropertyInfo.Name).IsModified = false;
                                break;
                        }
                        break;

                    case AutoGenerateValueType.LastModifiedDateTime:
                        switch (changedEntry.State)
                        {
                            case EntityState.Added:
                            case EntityState.Modified:
                                // set last modified date to current date/time
                                field.SetPropertyValue(changedEntry.Entity, now);
                                break;
                        }
                        break;
                }
            }
        }

        return base.SaveChanges();
    }
}