using System.ComponentModel.DataAnnotations;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace System;

public static class TypeExtensions
{
    public static bool IsSameOrSubclassOf(this Type potentialDescendant, Type potentialBase)
    {
        ArgumentNullException.ThrowIfNull(potentialBase);

        return potentialDescendant.IsSubclassOf(potentialBase)
               || potentialDescendant == potentialBase;
    }

    private static readonly HashSet<Type> NumericTypes = new()
    {
        // integer types
        typeof(byte), typeof(sbyte),
        typeof(short), typeof(ushort),
        typeof(int), typeof(uint),
        typeof(long), typeof(ulong),

        // float pointing numbers
        typeof(float), typeof(double), typeof(decimal)
    };

    // at compile time
#pragma warning disable CS0693
    public static bool IsNumericType<T>(this T input, bool includeNullable = true)
#pragma warning restore CS0693
    {
        var type = typeof(T);

        if (includeNullable)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                type = nullableType;
        }

        return NumericTypes.Contains(type);
    }

    // at runtime
    public static bool IsNumericType(this Type type, bool includeNullable = true)
    {
        if (includeNullable)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                type = nullableType;
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }

    public static string? GetDisplayAttributeName(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var displayAttribute = type.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            throw new NotSupportedException($"The type {type.FullName} does not implement attribute {nameof(DisplayAttribute)} which is required for the extension method {nameof(GetDisplayAttributeName)}.");

        var resourceManager = (Resources.ResourceManager?)displayAttribute.ResourceType?
            .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
            .Invoke(null, null);

        if (displayAttribute.Name == null)
            return type.FullName;

        return resourceManager?.GetString(displayAttribute.Name) ?? displayAttribute.Name;
    }

    public static string? GetDisplayAttributeNameOrDefault(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var displayAttribute = type.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute == null)
            return type.Name;

        var resourceManager = (Resources.ResourceManager?)displayAttribute.ResourceType?
            .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
            .Invoke(null, null);

        if (displayAttribute.Name == null)
            return type.FullName;

        return resourceManager?.GetString(displayAttribute.Name) ?? displayAttribute.Name;
    }

    // Source: https://stackoverflow.com/questions/74616/how-to-detect-if-type-is-another-generic-type/1075059#1075059
    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        ArgumentNullException.ThrowIfNull(givenType);

        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                return true;
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        Type? baseType = givenType.BaseType;
        if (baseType == null) return false;

        return IsAssignableToGenericType(baseType, genericType);
    }

    public static bool IsSubclassOfRawGeneric(this Type toCheck, Type genericType)
    {
        ArgumentNullException.ThrowIfNull(toCheck);

        while (toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (genericType == cur)
            {
                return true;
            }

            if (toCheck.BaseType == null)
                return false;

            toCheck = toCheck.BaseType;
        }

        return false;
    }

    /// <summary>
    /// [ <c>public static object GetDefault(Type type)</c> ]
    /// <para></para>
    /// Retrieves the default value for a given Type
    /// </summary>
    /// <param name="type">The Type for which to get the default value</param>
    /// <returns>The default value for <paramref name="type"/></returns>
    /// <remarks>
    /// If a null Type, a reference Type, or a System.Void Type is supplied, this method always returns null.  If a value type 
    /// is supplied which is not publicly visible or which contains generic parameters, this method will fail with an 
    /// exception.
    /// </remarks>
    public static object? GetTypeDefault(this Type? type)
    {
        // If no Type was supplied, if the Type was a reference type, or if the Type was a System.Void, return null
        if (type == null || !type.IsValueType || type == typeof(void))
            return null;

        // If the supplied Type has generic parameters, its default value cannot be determined
        if (type.ContainsGenericParameters)
            throw new ArgumentException(
                "{" + MethodBase.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
                "> contains generic parameters, so the default value cannot be retrieved");

        // If the Type is a primitive type, or if it is another publicly-visible value type (i.e. struct), return a 
        //  default instance of the value type
        if (type.IsPrimitive || !type.IsNotPublic)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                throw new ArgumentException(
                    "{" + MethodBase.GetCurrentMethod() +
                    "} Error:\n\nThe Activator.CreateInstance method could not " +
                    "create a default instance of the supplied value type <" + type +
                    "> (Inner Exception message: \"" + e.Message + "\")", e);
            }
        }

        // Fail with exception
        throw new ArgumentException("{" + MethodBase.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" +
                                    type +
                                    "> is not a publicly-visible type, so the default value cannot be retrieved");
    }


    #region GetMethodExt(..)

    // source: https://stackoverflow.com/questions/4035719/getmethod-for-generic-method

    /// <summary>
    /// Search for a method by name and parameter types.  
    /// Unlike GetMethod(), does 'loose' matching on generic
    /// parameter types, and searches base interfaces.
    /// </summary>
    /// <exception cref="AmbiguousMatchException"/>
    public static MethodInfo? GetMethodExt(this Type thisType,
        string name,
        params Type[] parameterTypes)
    {
        return GetMethodExt(thisType,
            name,
            BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.FlattenHierarchy,
            parameterTypes);
    }

    /// <summary>
    /// Search for a method by name, parameter types, and binding flags.  
    /// Unlike GetMethod(), does 'loose' matching on generic
    /// parameter types, and searches base interfaces.
    /// </summary>
    /// <exception cref="AmbiguousMatchException"/>
    public static MethodInfo? GetMethodExt(this Type thisType,
        string name,
        BindingFlags bindingFlags,
        params Type[] parameterTypes)
    {
        MethodInfo? matchingMethod = null;

        // Check all methods with the specified name, including in base classes
        GetMethodExt(ref matchingMethod, thisType, name, bindingFlags, parameterTypes);

        // If we're searching an interface, we have to manually search base interfaces
        if (matchingMethod == null && thisType.IsInterface)
        {
            foreach (Type interfaceType in thisType.GetInterfaces())
                GetMethodExt(ref matchingMethod,
                    interfaceType,
                    name,
                    bindingFlags,
                    parameterTypes);
        }

        return matchingMethod;
    }

    private static void GetMethodExt(ref MethodInfo? matchingMethod,
        Type type,
        string name,
        BindingFlags bindingFlags,
        params Type[] parameterTypes)
    {
        // Check all methods with the specified name, including in base classes
        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (MethodInfo methodInfo in type.GetMember(name,
                     MemberTypes.Method,
                     bindingFlags))
        {
            // Check that the parameter counts and types match, 
            // with 'loose' matching on generic parameters
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length == parameterTypes.Length)
            {
                int i = 0;
                for (; i < parameterInfos.Length; ++i)
                {
                    if (!parameterInfos[i].ParameterType
                            .IsSimilarType(parameterTypes[i]))
                        break;
                }
                if (i == parameterInfos.Length)
                {
                    if (matchingMethod == null)
                        matchingMethod = methodInfo;
                    else
                        throw new AmbiguousMatchException(
                            "More than one matching method found!");
                }
            }
        }
    }

    /// <summary>
    /// Special type used to match any generic parameter type in GetMethodExt().
    /// </summary>
    public class T
    { }

    /// <summary>
    /// Determines if the two types are either identical, or are both generic 
    /// parameters or generic types with generic parameters in the same
    ///  locations (generic parameters match any other generic paramter,
    /// but NOT concrete types).
    /// </summary>
    private static bool IsSimilarType(this Type thisType, Type type)
    {
        ArgumentNullException.ThrowIfNull(thisType);
        ArgumentNullException.ThrowIfNull(type);

#pragma warning disable CS8600
        // Ignore any 'ref' types
        if (thisType.IsByRef)
            thisType = thisType.GetElementType();
        if (type.IsByRef)
            type = type.GetElementType();
#pragma warning restore CS8600

        // ReSharper disable PossibleNullReferenceException
        // ReSharper disable AssignNullToNotNullAttribute
        // Handle array types
#pragma warning disable CS8602
        if (thisType.IsArray && type.IsArray)
#pragma warning disable CS8604
            return thisType.GetElementType().IsSimilarType(type.GetElementType());
#pragma warning restore CS8604
#pragma warning restore CS8602
        // ReSharper restore AssignNullToNotNullAttribute

        // If the types are identical, or they're both generic parameters 
        // or the special 'T' type, treat as a match
        if (thisType == type || (thisType.IsGenericParameter || thisType == typeof(T))
#pragma warning disable CS8602
            && (type.IsGenericParameter || type == typeof(T)))
#pragma warning restore CS8602
            return true;

        // Handle any generic arguments
#pragma warning disable CS8602
        if (thisType.IsGenericType && type.IsGenericType)
#pragma warning restore CS8602
        {
            Type[] thisArguments = thisType.GetGenericArguments();
            Type[] arguments = type.GetGenericArguments();
            if (thisArguments.Length == arguments.Length)
            {
                for (int i = 0; i < thisArguments.Length; ++i)
                {
                    if (!thisArguments[i].IsSimilarType(arguments[i]))
                        return false;
                }
                return true;
            }
        }
        // ReSharper restore PossibleNullReferenceException

        return false;
    }

    #endregion
}