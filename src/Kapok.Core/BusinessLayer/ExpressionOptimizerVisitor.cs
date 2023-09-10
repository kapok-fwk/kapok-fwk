using System.Linq.Expressions;

namespace Kapok.BusinessLayer;

/// <summary>
/// The Linq-to-SQL implementation in EF Core has some limitations
/// and with this ExpressionVisitor we optimize the Expression tree
/// so that it can be executed in SQL.
/// </summary>
public class ExpressionOptimizerVisitor : ExpressionVisitor
{
    public static ExpressionOptimizerVisitor Singleton => new();

    private ExpressionOptimizerVisitor()
    {
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        if (node.NodeType == ExpressionType.TypeAs &&
            node.Type.IsInterface &&
            node.Type.IsAssignableFrom(node.Operand.Type))
        {
            // input: (MyType value) => (IMyInterface)value;
            // 
            // if the condition is given that IMyInterface is implemented by MyType
            // the casting is removed.
            //
            // output: (MyType value) => value;

            // this cast is not anymore required and can be removed
            return node.Operand;
        }

        return base.VisitUnary(node);
    }
}