using System.Reflection;

namespace Kapok.BusinessLayer;

public class EntityNotFoundByKeyException : BusinessLayerException
{
    public EntityNotFoundByKeyException(Type entityType, IDictionary<PropertyInfo, object?> keyValues)
    {
        EntityType = entityType;
        KeyValues = keyValues;
    }

    public Type EntityType { get; }

    public IDictionary<PropertyInfo, object?> KeyValues { get; }

    public override string Message => string.Format(
        "Could not find {0} by key: {1}", // TODO: translation missing
        EntityType.GetDisplayAttributeNameOrDefault(),
        string.Join(", ", KeyValues.Select(p => string.Format("{0}={2}{1}{2}",
            p.Key.GetDisplayAttributeNameOrDefault(),
            p.Value,
            p.Key.PropertyType == typeof(string) ? "'" : "")
        ))
    );
}