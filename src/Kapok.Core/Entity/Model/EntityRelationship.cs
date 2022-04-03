using System.Linq.Expressions;
using System.Reflection;

namespace Kapok.Entity.Model;

public class EntityRelationship : IEntityRelationship
{
    public string Name { get; set; }
    public Type PrincipalEntityType { get; set; }
    public Type DependentEntityType { get; set; }
    public List<PropertyInfo> PrincipalKeyProperties { get; set; }
    public List<PropertyInfo> ForeignKeyProperties { get; set; }
    public RelationshipType RelationshipType { get; set; }
    public DeleteBehavior DeleteBehavior { get; set; }
        
    public PropertyInfo PrincipalNavigationProperty { get; set; }
    public PropertyInfo ForeignNavigationProperty { get; set; }

    // TODO: generate this automatically
    public Expression MatchExpression { get; set; }

    IReadOnlyCollection<PropertyInfo> IEntityRelationship.PrincipalKeyProperties => PrincipalKeyProperties;
    IReadOnlyCollection<PropertyInfo> IEntityRelationship.ForeignKeyProperties => ForeignKeyProperties;

    public LambdaExpression? GenerateChildrenWherePartExpression(Expression currentEntityExpression)
    {
        var entityDestinationModel = EntityBase.GetEntityModel(DependentEntityType);

        // build lambda expression for e => e.Property == current.Property),
        var whereExpressionParam = Expression.Parameter(DependentEntityType, "e");
        Expression? whereExpression = null;
        for (int i = 0; i < ForeignKeyProperties.Count - 1; i++)
        {
            var principalPropertyInfo = entityDestinationModel.PrimaryKeyProperties[i];
            var fkPropertyInfo = ForeignKeyProperties[i];

            if (principalPropertyInfo.PropertyType != fkPropertyInfo.PropertyType)
            {
                throw new NotSupportedException("Internal: property type in where clause is not equal");
            }

            BinaryExpression equalExpression = Expression.Equal(
                Expression.Property(whereExpressionParam, principalPropertyInfo),
                Expression.Property(currentEntityExpression, fkPropertyInfo)
            );

            if (whereExpression == null)
                whereExpression = equalExpression;
            else
                whereExpression = Expression.And(whereExpression, equalExpression);
        }

        if (whereExpression == null)
            return null;

        var whereLambdaExpression = Expression.Lambda(whereExpression, whereExpressionParam);
        return whereLambdaExpression;
    }

    public Func<TEntity, bool> GenerateChildrenWherePredicate<TEntity>(TEntity currentEntity)
        where TEntity : class
    {
        var currentEntityExpression = Expression.Constant(currentEntity);
        var whereLambdaExpression = GenerateChildrenWherePartExpression(currentEntityExpression);
        return (Func<TEntity, bool>) whereLambdaExpression.Compile();
    }
}