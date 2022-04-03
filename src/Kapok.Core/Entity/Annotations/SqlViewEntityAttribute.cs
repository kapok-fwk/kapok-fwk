namespace Kapok.Entity;

/// <summary>
/// Used for an entity to give information to the migration
/// logic that this entity is directing to a view, not to a
/// actual table, so, the migration will not create a table.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SqlViewEntityAttribute : Attribute
{
}