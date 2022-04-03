using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Kapok.Entity.Model;

public sealed class EntityModelBuilder<T>
    where T : class
{
    internal EntityModel Model { get; }

    /// <summary>
    /// Creates a new model builder. When <para>model</para> is set the given model is extended.
    /// Otherwise a new model is created with the model builder.
    /// </summary>
    /// <param name="model"></param>
    internal EntityModelBuilder(EntityModel? model = null)
    {
        if (model == null)
        {
            Model = new EntityModel(typeof(T));
            BasisModelCreation();
        }
        else
        {
            Model = model;
        }
    }

    private void BasisModelCreation()
    {
        // add auto-calculate properties from attributes
        {
            var keyProperties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                .Where(p => Attribute.IsDefined(p, typeof(KeyAttribute))).ToArray();

            if (keyProperties.Length > 0)
            {
                this.SetPrimaryKey(keyProperties);
            }

            var autoCalculateProperties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Where(p => Attribute.IsDefined(p, typeof(AutoCalculateAttribute)));

            var allowedTypesForAutoCalculateParameter = new List<Type>(new []{ typeof(IReadOnlyDictionary<string, object>) });

            foreach (var autoCalculateProperty in autoCalculateProperties)
            {
                var attr = autoCalculateProperty.GetCustomAttribute<AutoCalculateAttribute>();

                var methodName = attr?.MethodName ?? $"AutoCalculate{autoCalculateProperty.Name}";

                var methodInfo = typeof(T).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);

                if (methodInfo == null)
                    throw new NotSupportedException($"The method '{methodName}' could not be found in the type {typeof(T).FullName}. The method is required for the auto-calculation of the property {autoCalculateProperty.Name}. The method must be static and public.");

                var returnParameterType = typeof(Expression<>).MakeGenericType(
                    typeof(Func<,>).MakeGenericType(typeof(T), autoCalculateProperty.PropertyType));

                if (methodInfo.ReturnType != returnParameterType)
                    throw new NotSupportedException(
                        $"The return value of the method {methodName} must be of the type {returnParameterType.Name}");

                var methodParameters = methodInfo.GetParameters();

                if (methodParameters.Length == 0)
                {
                    var expression = methodInfo.Invoke(null, null);

                    var propertyCalculateDefinitionType =
                        typeof(PropertyCalculateDefinition<,>).MakeGenericType(typeof(T),
                            autoCalculateProperty.PropertyType);

                    var propertyCalculateDefinition =
                        (IPropertyCalculateDefinition?)Activator.CreateInstance(propertyCalculateDefinitionType,
                            expression);

                    GetProperty(autoCalculateProperty)
                        .AddCalculation(propertyCalculateDefinition);
                }
                else
                {
                    foreach (var methodParameter in methodParameters)
                    {
                        if (!allowedTypesForAutoCalculateParameter.Contains(methodParameter.ParameterType))
                            throw new NotSupportedException(
                                $"The method {methodName} has a parameter with an not supported type {methodParameter.ParameterType} for an auto-calculated method.");
                    }

                    var propertyBuilder = GetProperty(autoCalculateProperty);

                    var addCalculationMethod = propertyBuilder.GetType().GetMethod(
                        nameof(PropertyModelBuilder<T>.AddCalculation),
                        BindingFlags.Instance | BindingFlags.Public, null, new[] {typeof(MethodInfo)}, null);
                    if (addCalculationMethod == null)
                        throw new MissingMethodException(typeof(PropertyModelBuilder<T>).FullName, nameof(PropertyModelBuilder<T>.AddCalculation));
                    // ReSharper disable once PossibleNullReferenceException
                    var newMethodCall = addCalculationMethod.MakeGenericMethod(autoCalculateProperty.PropertyType);

                    newMethodCall.Invoke(propertyBuilder, new object[] {methodInfo});
                }
            }
        }
            
    }

    internal static PropertyInfo[] GetPropertyInfosFromNames(Type entityType, string[] propertyNames)
    {
        var propertyInfoList = new PropertyInfo[propertyNames.Length];

        for (var index = 0; index < propertyNames.Length; index++)
        {
            var propertyName = propertyNames[index];
            var propertyInfo = entityType.GetProperty(propertyName,
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);

            if (propertyInfo == null)
                throw new NotSupportedException($"Could not find public property {propertyName} in entity {entityType.FullName}");

            propertyInfoList[index] = propertyInfo;
        }

        return propertyInfoList;
    }

    public EntityModelBuilder<T> SetPrimaryKey(params PropertyInfo[]? properties)
    {
        if (properties == null || properties.Length == 0)
            throw new ArgumentNullException(nameof(properties));

        var type = typeof(T);

        if (Model.PrimaryKeyProperties != null)
            throw new NotSupportedException(
                $"A primary key is already set for type {type.FullName}");

        Model.PrimaryKeyProperties = properties;

        return this;
    }

    public EntityModelBuilder<T> SetPrimaryKey(params string[]? propertyNames)
    {
        if (propertyNames == null || propertyNames.Length == 0)
            throw new ArgumentNullException(nameof(propertyNames));

        var type = typeof(T);

        return SetPrimaryKey(GetPropertyInfosFromNames(type, propertyNames));
    }

    public PropertyModelBuilder<T> GetProperty(string propertyName)
    {
        var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

        if (propertyInfo == null)
            throw new NotSupportedException(
                $"The instance property {propertyName} does not exist in the type {typeof(T).FullName}");

        return GetProperty(propertyInfo);
    }

    public PropertyModelBuilder<T> GetProperty(PropertyInfo property)
    {
        var propertyModel = Model.Properties.FirstOrDefault(p => p.PropertyName == property.Name);

        if (propertyModel == null)
        {
            propertyModel = new EntityProperty(property.Name);

            Model.Properties.Add(propertyModel);
        }

        return new PropertyModelBuilder<T>(propertyModel);
    }

    public EntityModelBuilder<T> AddIndex(params string[]? propertyNames)
    {
        if (propertyNames == null || propertyNames.Length == 0)
            throw new ArgumentNullException(nameof(propertyNames));

        var propertyInfoList = GetPropertyInfosFromNames(typeof(T), propertyNames);

        Model.Indexes.Add(new IndexModel(propertyInfoList));

        return this;
    }

    public EntityModelBuilder<T> AddUniqueIndex(params string[]? propertyNames)
    {
        if (propertyNames == null || propertyNames.Length == 0)
            throw new ArgumentNullException(nameof(propertyNames));

        var propertyInfoList = GetPropertyInfosFromNames(typeof(T), propertyNames);

        Model.Indexes.Add(new IndexModel(propertyInfoList));

        return this;
    }

    public ReferenceModelBuilder<T, TDestinationType> AddOneToManyRelationship<TDestinationType>(string? navigationPropertyName = null, string? name = null)
        where TDestinationType : class, new()
    {
        PropertyInfo? navigationProperty = null;
        if (navigationPropertyName != null)
            navigationProperty = typeof(T).GetProperty(navigationPropertyName);
            
        name ??= navigationProperty?.Name ?? typeof(TDestinationType).Name;

        if (Model.References.Any(r => r.Name == name))
            throw new ArgumentException($"A reference with the name '{name}' exist already for this entity", nameof(name));

        var reference = new EntityRelationship
        {
            Name = name,
            PrincipalEntityType = typeof(T),
            PrincipalNavigationProperty = navigationProperty,
            DependentEntityType = typeof(TDestinationType),
            RelationshipType = RelationshipType.OneToMany,
            DeleteBehavior = DeleteBehavior.Restrict
        };

        Model.References.Add(reference);

        return new ReferenceModelBuilder<T, TDestinationType>(reference, Model);
    }

    public ReferenceModelBuilder<T, TDestinationType> AddOneToOneRelationship<TDestinationType>(string? navigationPropertyName = null, string? name = null)
        where TDestinationType : class, new()
    {
        PropertyInfo? navigationProperty = null;
        if (navigationPropertyName != null)
            navigationProperty = typeof(T).GetProperty(navigationPropertyName);

        name ??= navigationProperty?.Name ?? typeof(TDestinationType).Name;
            
        if (Model.References.Any(r => r.Name == name))
            throw new ArgumentException($"A reference with the name '{name}' exist already for this entity", nameof(name));

        var reference = new EntityRelationship
        {
            Name = name,
            PrincipalEntityType = typeof(T),
            PrincipalNavigationProperty = navigationProperty,
            DependentEntityType = typeof(TDestinationType),
            RelationshipType = RelationshipType.OneToOne,
            DeleteBehavior = DeleteBehavior.Restrict
        };

        Model.References.Add(reference);

        return new ReferenceModelBuilder<T, TDestinationType>(reference, Model);
    }

    public ReferenceModelBuilder<T, TDestinationType> AddManyToOneRelationship<TDestinationType>(string? name = null, string? navigationPropertyName = null)
        where TDestinationType : class, new()
    {
        PropertyInfo? navigationProperty = null;
        if (navigationPropertyName != null)
            navigationProperty = typeof(T).GetProperty(navigationPropertyName);
            
        name ??= navigationProperty?.Name ?? typeof(TDestinationType).Name;

        if (Model.References.Any(r => r.Name == name))
            throw new ArgumentException($"A reference with the name '{name}' exist already for this entity", nameof(name));

        var reference = new EntityRelationship
        {
            Name = name,
            PrincipalEntityType = typeof(T),
            PrincipalNavigationProperty = navigationProperty,
            DependentEntityType = typeof(TDestinationType),
            RelationshipType = RelationshipType.ManyToOne,
            DeleteBehavior = DeleteBehavior.Restrict
        };

        Model.References.Add(reference);

        return new ReferenceModelBuilder<T, TDestinationType>(reference);
    }
}