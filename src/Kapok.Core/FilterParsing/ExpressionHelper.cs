using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace Kapok.Core.FilterParsing;

internal class ExpressionHelper : IExpressionHelper
{
    public Expression GenerateAdd(Expression left, Expression right)
    {
        return Expression.Add(left, right);
    }

    public Expression GenerateStringConcat(Expression left, Expression right)
    {
        return GenerateStaticMethodCall("Concat", left, right);
    }

    public Expression GenerateSubtract(Expression left, Expression right)
    {
        return Expression.Subtract(left, right);
    }

    internal static string? TransformLikeStringToSqlLike(string? likeString)
    {
        if (likeString == null)
            return null;

        if (likeString == string.Empty)
            return null;

        var sqlLikeString = new StringBuilder();

        int pos = 0;
        while(pos < likeString.Length)
        {
            char c = likeString[pos++];

            if (c == '[')
            {
                sqlLikeString.Append("[[]");
            }
            else if (c == ']')
            {
                sqlLikeString.Append("[]]");
            }
            else if (c == '%')
            {
                sqlLikeString.Append("[%]");
            }
            else if (c == '_')
            {
                sqlLikeString.Append("[_]");
            }
            else if (c == '*')
            {
                sqlLikeString.Append('%');
            }
            else if (c == '?')
            {
                sqlLikeString.Append("_");
            }
            else
            {
                sqlLikeString.Append(c);
            }
        }

        return sqlLikeString.ToString();
    }

    public Expression GenerateLike(Expression left, Expression right)
    {
        // TODO: this probably only works when it is passed to EF core!

        var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like", new[] { typeof(DbFunctions), typeof(string), typeof(string) });
        if (likeMethod == null)
            throw new NotSupportedException("Could not find the EF core method Like");

        if (right.Type == typeof(string) && right.NodeType == ExpressionType.Constant)
        {
            string? value = (string?)((ConstantExpression)right).Value;
            right = Expression.Constant(TransformLikeStringToSqlLike(value), typeof(string));
        }

        return Expression.Call(null, likeMethod, Expression.Constant(EF.Functions), left, right);
    }

    public Expression GenerateNotLike(Expression left, Expression right)
    {
        return Expression.Not(GenerateLike(left, right));
    }

    public Expression GenerateEqual(Expression left, Expression right)
    {
        OptimizeForEqualityIfPossible(ref left, ref right);

        return Expression.Equal(left, right);
    }

    public Expression GenerateNotEqual(Expression left, Expression right)
    {
        OptimizeForEqualityIfPossible(ref left, ref right);

        return Expression.NotEqual(left, right);
    }

    public Expression GenerateGreaterThan(Expression left, Expression right)
    {
        if (left.Type == typeof(string))
        {
            return Expression.GreaterThan(GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
        }

        if (left.Type.GetTypeInfo().IsEnum || right.Type.GetTypeInfo().IsEnum)
        {
            var leftPart = left.Type.GetTypeInfo().IsEnum ? Expression.Convert(left, Enum.GetUnderlyingType(left.Type)) : left;
            var rightPart = right.Type.GetTypeInfo().IsEnum ? Expression.Convert(right, Enum.GetUnderlyingType(right.Type)) : right;
            return Expression.GreaterThan(leftPart, rightPart);
        }

        return Expression.GreaterThan(left, right);
    }

    public Expression GenerateGreaterThanEqual(Expression left, Expression right)
    {
        if (left.Type == typeof(string))
        {
            return Expression.GreaterThanOrEqual(GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
        }

        if (left.Type.GetTypeInfo().IsEnum || right.Type.GetTypeInfo().IsEnum)
        {
            return Expression.GreaterThanOrEqual(left.Type.GetTypeInfo().IsEnum ? Expression.Convert(left, Enum.GetUnderlyingType(left.Type)) : left,
                right.Type.GetTypeInfo().IsEnum ? Expression.Convert(right, Enum.GetUnderlyingType(right.Type)) : right);
        }

        return Expression.GreaterThanOrEqual(left, right);
    }

    public Expression GenerateLessThan(Expression left, Expression right)
    {
        if (left.Type == typeof(string))
        {
            return Expression.LessThan(GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
        }

        if (left.Type.GetTypeInfo().IsEnum || right.Type.GetTypeInfo().IsEnum)
        {
            return Expression.LessThan(left.Type.GetTypeInfo().IsEnum ? Expression.Convert(left, Enum.GetUnderlyingType(left.Type)) : left,
                right.Type.GetTypeInfo().IsEnum ? Expression.Convert(right, Enum.GetUnderlyingType(right.Type)) : right);
        }

        return Expression.LessThan(left, right);
    }

    public Expression GenerateLessThanEqual(Expression left, Expression right)
    {
        if (left.Type == typeof(string))
        {
            return Expression.LessThanOrEqual(GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
        }

        if (left.Type.GetTypeInfo().IsEnum || right.Type.GetTypeInfo().IsEnum)
        {
            return Expression.LessThanOrEqual(left.Type.GetTypeInfo().IsEnum ? Expression.Convert(left, Enum.GetUnderlyingType(left.Type)) : left,
                right.Type.GetTypeInfo().IsEnum ? Expression.Convert(right, Enum.GetUnderlyingType(right.Type)) : right);
        }

        return Expression.LessThanOrEqual(left, right);
    }

    public void OptimizeForEqualityIfPossible(ref Expression left, ref Expression right)
    {
        // The goal here is to provide the way to convert some types from the string form in a way that is compatible with Linq to Entities.
        // The Expression.Call(typeof(Guid).GetMethod("Parse"), right); does the job only for Linq to Object but Linq to Entities.
        Type leftType = left.Type;
        Type rightType = right.Type;

        if (rightType == typeof(string) && right.NodeType == ExpressionType.Constant)
        {
            right = OptimizeStringForEqualityIfPossible((string)((ConstantExpression)right).Value, leftType) ?? right;
        }

        if (leftType == typeof(string) && left.NodeType == ExpressionType.Constant)
        {
            left = OptimizeStringForEqualityIfPossible((string)((ConstantExpression)left).Value, rightType) ?? left;
        }
    }

    public Expression OptimizeStringForEqualityIfPossible(string text, Type type)
    {
        if (type == typeof(DateTime) && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
        {
            return Expression.Constant(dateTime, typeof(DateTime));
        }
#if !NET35
        if (type == typeof(Guid) && Guid.TryParse(text, out Guid guid))
        {
            return Expression.Constant(guid, typeof(Guid));
        }
#else
            try
            {
                return Expression.Constant(new Guid(text));
            }
            catch
            {
                // Doing it in old fashion way when no TryParse interface was provided by .NET
            }
#endif
        return null;
    }

    private MethodInfo GetStaticMethod(string methodName, Expression left, Expression right)
    {
        var methodInfo = left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
        if (methodInfo == null)
        {
            methodInfo = right.Type.GetMethod(methodName, new[] { left.Type, right.Type });
        }

        return methodInfo;
    }

    private Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
    {
        return Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
    }
}