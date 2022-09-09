using System.Reflection;

namespace Kapok.Core;

public abstract class DataDomain : IDataDomain
{
    private struct RegisteredEntity
    {
        public Type EntityType { get; set; }
        public Type DaoType { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsVirtual { get; set; }
    }

    #region Static

    private static readonly Dictionary<Type, RegisteredEntity> Entities = new();

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

            _defaultDaoType = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    #region Repository registration

    public static void RegisterEntity<T>(bool isReadOnly = false, bool isVirtual = false)
        where T : class, new()
    {
        if (!isVirtual)
            DataEntities.Add(typeof(T));

        Entities.Add(typeof(T), new RegisteredEntity
        {
            EntityType = typeof(T),
            DaoType = DefaultDaoType.MakeGenericType(typeof(T)),
            IsReadOnly = isReadOnly,
            IsVirtual = isVirtual
        });
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
                Entities.Add(typeof(TEntity), new RegisteredEntity
                {
                    EntityType = typeof(TEntity),
                    DaoType = typeof(TDao),
                    IsReadOnly = false,
                    IsVirtual = isVirtual
                });

                return;
            }
        }

        throw new NotSupportedException($"The dao type {typeof(TDao).FullName} has no public constructor with a list of supported parameters for initialization by this DataDomain.");
    }

    internal static IDao<T> ConstructNewDao<T>(IDataDomainScope dataDomainScope, IRepository<T> repository)
        where T : class, new()
    {
        var entityType = typeof(T);

        if (Entities.ContainsKey(entityType))
        {
            foreach (var constructorInfo in Entities[entityType].DaoType.GetConstructors())
            {
                var parameters = constructorInfo.GetParameters();
                var parameterValues = new object[parameters.Length];

                int i = 0;
                bool correct = true;
                foreach (var parameterInfo in parameters)
                {
                    if (parameterInfo.ParameterType == typeof(IDataDomainScope))
                    {
                        parameterValues[i] = dataDomainScope;
                    }
                    else if (parameterInfo.ParameterType == typeof(IRepository<T>))
                    {
                        parameterValues[i] = repository;
                    }
                    else if (parameterInfo.ParameterType == typeof(bool) &&
                             Equals(parameterInfo.Name, "isReadOnly"))
                    {
                        parameterValues[i] = Entities[entityType].IsReadOnly;
                    }
                    else if (parameterInfo.HasDefaultValue)
                    {
                        parameterValues[i] = parameterInfo.DefaultValue;
                    }
                    else
                    {
                        correct = false;
                        break;
                    }

                    i++;
                }

                if (correct)
                {
                    return (IDao<T>)constructorInfo.Invoke(parameterValues);
                }
            }
        }

        return null;
    }

    #endregion

    #endregion

    protected DataDomain()
    {
        Default ??= this;
    }

    private readonly Dictionary<string, DataPartition> _dataPartitions = new();

    public void RegisterDataPartition(string name, Type interfaceType, string propertyName)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

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