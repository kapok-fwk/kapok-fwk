using System.Reflection;
using Kapok.BusinessLayer;
using Microsoft.Extensions.DependencyInjection;

namespace Kapok.Data;

public abstract class DataDomain : IDataDomain
{
    internal record RegisteredEntity
    {
        public RegisteredEntity(Type entityType, Type daoType, bool isReadOnly, bool isVirtual, Type? contractType = null)
        {
            EntityType = entityType;
            ContractType = contractType;
            DaoType = daoType;
            IsReadOnly = isReadOnly;
            IsVirtual = isVirtual;
        }

        // ReSharper disable once NotAccessedField.Local
        public readonly Type EntityType;
        public readonly Type? ContractType;
        public readonly Type DaoType;
        public readonly bool IsReadOnly;
        // ReSharper disable once NotAccessedField.Local
        public readonly bool IsVirtual;
    }

    #region Static

    private static readonly Dictionary<Type, RegisteredEntity> _entities = new();

    internal static IReadOnlyDictionary<Type, RegisteredEntity> Entities => _entities;

    /// <summary>
    /// Get all entities which are saved into the database (= not virtual)
    /// </summary>
    public static List<Type> DataEntities { get; } = new();

    public static IDataDomain? Default { get; set; }

    private static Type _defaultDaoType = typeof(Dao<>);
    public static Type DefaultDaoType
    {
        get => _defaultDaoType;
        set
        {
            if (_defaultDaoType == value)
                return;

            ArgumentNullException.ThrowIfNull(value, nameof(value));
            _defaultDaoType = value;
        }
    }

    #region Repository registration

    public static void RegisterEntity<T>(bool isReadOnly = false, bool isVirtual = false)
        where T : class, new()
    {
        if (!isVirtual)
            DataEntities.Add(typeof(T));

        _entities.Add(typeof(T), new RegisteredEntity(
            entityType: typeof(T),
            daoType: DefaultDaoType.MakeGenericType(typeof(T)),
            isReadOnly: isReadOnly,
            isVirtual: isVirtual));
    }

    public static void RegisterEntity<TEntity, TDao>(bool isVirtual = false)
        where TEntity : class, new()
        where TDao : IReadOnlyDao<TEntity>
    {
        var supportedParameterTypes = new List<Type>
        {
            typeof(IDataDomainScope),
            typeof(IRepository<TEntity>)
        };

        if (!isVirtual)
            DataEntities.Add(typeof(TEntity));

        foreach (var constructorInfo in typeof(TDao).GetConstructors())
        {
            if (constructorInfo.GetParameters()
                .Any(p => supportedParameterTypes.Contains(p.ParameterType) || p.HasDefaultValue))
            {
                _entities.Add(typeof(TEntity), new RegisteredEntity(
                    entityType: typeof(TEntity),
                    daoType: typeof(TDao),
                    isReadOnly: false,
                    isVirtual: isVirtual));

                return;
            }
        }

        throw new NotSupportedException($"The dao type {typeof(TDao).FullName} has no public constructor with a list of supported parameters for initialization by this DataDomain.");
    }

    public static void RegisterEntity<TEntity, TService, TDao>(bool isVirtual = false)
        where TEntity : class, new()
        where TService : IReadOnlyDao<TEntity>
        where TDao : IReadOnlyDao<TEntity>
    {
        var supportedParameterTypes = new List<Type>
        {
            typeof(IDataDomainScope),
            typeof(IRepository<TEntity>)
        };

        if (!isVirtual)
            DataEntities.Add(typeof(TEntity));

        foreach (var constructorInfo in typeof(TDao).GetConstructors())
        {
            if (constructorInfo.GetParameters()
                .Any(p => supportedParameterTypes.Contains(p.ParameterType) || p.HasDefaultValue))
            {
                _entities.Add(typeof(TEntity), new RegisteredEntity(
                    entityType: typeof(TEntity),
                    daoType: typeof(TDao),
                    isReadOnly: false,
                    isVirtual: isVirtual,
                    contractType: typeof(TService)));

                return;
            }
        }

        throw new NotSupportedException($"The dao type {typeof(TDao).FullName} has no public constructor with a list of supported parameters for initialization by this DataDomain.");
    }

    internal static IDao<T> ConstructNewDao<T>(IServiceProvider serviceProvider, IRepository<T> repository)
        where T : class, new()
    {
        var entityType = typeof(T);

        if (!_entities.ContainsKey(entityType))
            throw new ArgumentException(
                $"The passed generic type {typeof(T).FullName} is not registered as entity. The DAO object cannot be created.");

        var dao = (IDao<T>?)serviceProvider.GetService(_entities[entityType].DaoType);
        if (dao != null) return dao;

        if (_entities[entityType].ContractType != null)
        {
            dao = (IDao<T>?)serviceProvider.GetService(_entities[entityType].ContractType);
            if (dao != null) return dao;
        }

        dao = (IDao<T>)ActivatorUtilities.CreateInstance(serviceProvider, _entities[entityType].DaoType);

        return dao;
    }

    #endregion

    #endregion

    private IServiceProvider? _serviceProvider;

    protected DataDomain()
    {
        Default ??= this;
    }

    protected DataDomain(IServiceProvider? serviceProvider)
        : this()
    {
        _serviceProvider = serviceProvider;
    }

    protected virtual void ConfigureServices(IServiceCollection serviceCollection)
    {
    }

    /// <summary>
    /// The service provider to be used for page construction.
    /// </summary>
    public IServiceProvider ServiceProvider
    {
        get => _serviceProvider ??= CreateDefaultServiceProvider();
        set => _serviceProvider = value;
    }

    private IServiceProvider CreateDefaultServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDataDomain>(p => this);
        ConfigureServices(services);
        return services.BuildServiceProvider();
    }

    private readonly Dictionary<string, DataPartition> _dataPartitions = new();

    public void RegisterDataPartition(string name, Type interfaceType, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        if (!interfaceType.IsInterface)
            throw new ArgumentException("The type must be an interface", nameof(interfaceType));

        var dataScopeProperty = interfaceType.GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty
        );

        if (dataScopeProperty == null)
        {
            throw new ArgumentException("The data scope property must be a public instance property with a getter and setter of the interface", nameof(propertyName));
        }

        if (_dataPartitions.ContainsKey(name))
        {
            throw new ArgumentException($"A data scope with name {name} is already added to the data domain.");
        }

        _dataPartitions.Add(name, new DataPartition(interfaceType, dataScopeProperty));
    }

    public IReadOnlyDictionary<string, DataPartition> DataPartitions => _dataPartitions;

    public abstract IDataDomainScope CreateScope();
}