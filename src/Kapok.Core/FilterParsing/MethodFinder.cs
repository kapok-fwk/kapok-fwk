using System.Linq.Expressions;
using System.Reflection;

namespace Kapok.BusinessLayer.FilterParsing;

internal class MethodFinder
{
    private readonly IExpressionPromoter _expressionPromoter;

    /// <summary>
    /// Get an instance
    /// </summary>
    public MethodFinder(IExpressionPromoter expressionPromoter)
    {
        _expressionPromoter = expressionPromoter;
    }

    public bool ContainsMethod(Type type, string methodName, bool staticAccess, Expression[] args)
    {
        return FindMethod(type, methodName, staticAccess, args, out _) == 1;
    }

    public int FindMethod(Type type, string methodName, bool staticAccess, Expression[] args, out MethodBase? method)
    {
        BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly | (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
        foreach (Type t in SelfAndBaseTypes(type))
        {
            MemberInfo[] members = t.FindMembers(MemberTypes.Method, flags, Type.FilterNameIgnoreCase, methodName);
            int count = FindBestMethod(members.Cast<MethodBase>(), args, out method);
            if (count != 0)
            {
                return count;
            }
        }
        method = null;
        return 0;
    }

    public int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase? method)
    {
        MethodData[] applicable = methods.
            Select(m => new MethodData(m, m.GetParameters())).
            Where(m => IsApplicable(m, args)).
            ToArray();

        if (applicable.Length > 1)
        {
            applicable = applicable.Where(m => applicable.All(n => m == n || IsBetterThan(args, m, n))).ToArray();
        }

        if (args.Length == 2 && applicable.Length > 1 && (args[0].Type == typeof(Guid?) || args[1].Type == typeof(Guid?)))
        {
            applicable = applicable.Take(1).ToArray();
        }

        if (applicable.Length == 1)
        {
            MethodData md = applicable[0];
            if (md.Args == null)
            {
                if (args.Length > 0)
                    // the MethodData object has not enough parameters as parameter args provides
                    throw new ArgumentOutOfRangeException();
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = md.Args[i];
                }
            }

            method = md.MethodBase;
        }
        else
        {
            method = null;
        }

        return applicable.Length;
    }

    bool IsApplicable(MethodData method, Expression[] args)
    {
        if (method.Parameters.Length != args.Length)
        {
            return false;
        }

        Expression[] promotedArgs = new Expression[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            ParameterInfo pi = method.Parameters[i];
            if (pi.IsOut)
            {
                return false;
            }

            Expression? promoted = _expressionPromoter.Promote(args[i], pi.ParameterType, false, true);
            if (promoted == null)
            {
                return false;
            }
            promotedArgs[i] = promoted;
        }
        method.Args = promotedArgs;
        return true;
    }

    bool IsBetterThan(Expression[] args, MethodData first, MethodData second)
    {
        bool better = false;
        for (int i = 0; i < args.Length; i++)
        {
            CompareConversionType result = CompareConversions(args[i].Type, first.Parameters[i].ParameterType, second.Parameters[i].ParameterType);

            // If second is better, return false
            if (result == CompareConversionType.Second)
            {
                return false;
            }

            // If first is better, return true
            if (result == CompareConversionType.First)
            {
                return true;
            }

            // If both are same, just set better to true and continue
            if (result == CompareConversionType.Both)
            {
                better = true;
            }
        }

        return better;
    }

    // Return "First" if s -> t1 is a better conversion than s -> t2
    // Return "Second" if s -> t2 is a better conversion than s -> t1
    // Return "Both" if neither conversion is better
    CompareConversionType CompareConversions(Type source, Type first, Type second)
    {
        if (first == second)
        {
            return CompareConversionType.Both;
        }
        if (source == first)
        {
            return CompareConversionType.First;
        }
        if (source == second)
        {
            return CompareConversionType.Second;
        }

        bool firstIsCompatibleWithSecond = TypeHelper.IsCompatibleWith(first, second);
        bool secondIsCompatibleWithFirst = TypeHelper.IsCompatibleWith(second, first);

        if (firstIsCompatibleWithSecond && !secondIsCompatibleWithFirst)
        {
            return CompareConversionType.First;
        }
        if (secondIsCompatibleWithFirst && !firstIsCompatibleWithSecond)
        {
            return CompareConversionType.Second;
        }

        if (TypeHelper.IsSignedIntegralType(first) && TypeHelper.IsUnsignedIntegralType(second))
        {
            return CompareConversionType.First;
        }
        if (TypeHelper.IsSignedIntegralType(second) && TypeHelper.IsUnsignedIntegralType(first))
        {
            return CompareConversionType.Second;
        }

        return CompareConversionType.Both;
    }

    IEnumerable<Type> SelfAndBaseTypes(Type type)
    {
        if (type.IsInterface)
        {
            var types = new List<Type>();
            AddInterface(types, type);
            return types;
        }
        return SelfAndBaseClasses(type);
    }

    IEnumerable<Type> SelfAndBaseClasses(Type type)
    {
        Type? iterateType = type;

        do
        {
            yield return iterateType;
            iterateType = type.GetTypeInfo().BaseType;
        } while (iterateType != null);
    }

    void AddInterface(List<Type> types, Type type)
    {
        if (!types.Contains(type))
        {
            types.Add(type);
            foreach (Type t in type.GetInterfaces()) AddInterface(types, t);
        }
    }
}