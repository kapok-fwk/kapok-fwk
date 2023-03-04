namespace Kapok.BusinessLayer;

public class EntityNoPrimaryKeyException : BusinessLayerException
{
    private readonly string? _additionalMessage;

    public EntityNoPrimaryKeyException(Type entityType, string? additionalMessage = null)
    {
        EntityType = entityType;
        _additionalMessage = additionalMessage;
    }

    public Type EntityType { get; }

    public override string Message => string.Format(
        "Entity {0} does not have a primary key.{1}", // TODO: translation missing
        EntityType.GetDisplayAttributeName(),
        string.IsNullOrEmpty(_additionalMessage) ? "" : $"\n{_additionalMessage}"
    );
}