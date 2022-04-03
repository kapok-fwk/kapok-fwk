namespace Kapok.Entity.Model;

public class ModelBuilder
{
    internal ModelBuilder(Type type)
    {
        _baseType = type;
    }

    // NOTE: as of today this model builder is only used for one entity type. This might change in the future
    private readonly Type _baseType;

    public ModelBuilder Entity<T>(Action<EntityModelBuilder<T>>? action)
        where T : class
    {
        if (typeof(T) != _baseType)
            throw new NotSupportedException($"The generic type T must be the type {_baseType.FullName}.");

        var entityModelBuilder = new EntityModelBuilder<T>((EntityModel?)Model);

        action?.Invoke(entityModelBuilder);

        Model = entityModelBuilder.Model;

        return this;
    }

    public IEntityModel? Model { get; private set; }
}