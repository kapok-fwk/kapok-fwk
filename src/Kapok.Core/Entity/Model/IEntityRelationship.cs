using System.Linq.Expressions;
using System.Reflection;

namespace Kapok.Entity.Model;

public interface IEntityRelationship
{
    public string Name { get; }
    public Type PrincipalEntityType { get; }
    public Type DependentEntityType { get; }

    IReadOnlyCollection<PropertyInfo>? PrincipalKeyProperties { get; }
    IReadOnlyCollection<PropertyInfo>? ForeignKeyProperties { get; }

    RelationshipType RelationshipType { get; }
    DeleteBehavior DeleteBehavior { get; }
        
    PropertyInfo? PrincipalNavigationProperty { get; }
    PropertyInfo? ForeignNavigationProperty { get; }

    /// <summary>
    /// A lambda expression (sourceType, destinationType) => return boolean
    /// which matches if the entities reference each other
    /// </summary>
    Expression? MatchExpression { get; }

    Func<TEntity, bool>? GenerateChildrenWherePredicate<TEntity>(TEntity currentEntity)
        where TEntity : class;
}