﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace System;

public static class EnumExtension
{
    static void TestIsEnumType(Type enumType)
    {
        if (enumType == null) throw new ArgumentNullException(nameof(enumType));
        if (!enumType.IsEnum)
            throw new ArgumentException("TEnum must be an enum.");
    }

    public static string EnumValueToDisplayName(object enumValue, CultureInfo? cultureInfo = null)
    {
        TestIsEnumType(enumValue.GetType());

        var enumNameMemberInfo = enumValue.GetType()
            .GetMember(enumValue.ToString())
            .FirstOrDefault();

        var displayAttribute = enumNameMemberInfo?.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute != null)
        {
            if (displayAttribute.ResourceType != null)
            {
                ResourceManager? resourceManager = (ResourceManager?) displayAttribute.ResourceType?
                    .GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static)?.GetMethod?
                    .Invoke(null, null);

                if (resourceManager != null)
                {
                    var switchCulture = cultureInfo != null &&
                                        !Thread.CurrentThread.CurrentUICulture.Equals(cultureInfo);
                    CultureInfo? oldCultureInfo = null;

                    if (switchCulture)
                    {
                        Thread.CurrentThread.CurrentUICulture = cultureInfo;
                        oldCultureInfo = Thread.CurrentThread.CurrentUICulture;
                    }

                    var resourceString = resourceManager.GetString(displayAttribute.Name);

                    if (switchCulture)
                    {
                        Thread.CurrentThread.CurrentUICulture = oldCultureInfo;
                    }

                    return resourceString ?? displayAttribute.Name;
                }
            }

            return displayAttribute.Name;
        }

        var displayNameAttribute = enumNameMemberInfo?.GetCustomAttribute<DisplayNameAttribute>();
        if (displayNameAttribute != null)
        {
            return displayNameAttribute.DisplayName;
        }

        return enumValue.ToString();
    }

    public static string ToDisplayName<TEnum>(this TEnum @enum, CultureInfo? cultureInfo = null)
        where TEnum : Enum
    {
        return EnumValueToDisplayName(@enum, cultureInfo);
    }

    public static object ParseDisplayName(Type enumType, string s, CultureInfo? cultureInfo = null)
    {
        if (TryParseDisplayName(enumType, s, out var result, cultureInfo))
            return result;
        throw new NotSupportedException($"Could not parse string {s} to an enum value of enum type {enumType.FullName}");
    }

    public static TEnum ParseDisplayName<TEnum>(string s, CultureInfo? cultureInfo = null)
        where TEnum : struct, IConvertible, IComparable, IFormattable
    {
        return (TEnum) ParseDisplayName(typeof(TEnum), s, cultureInfo);
    }

    public static bool TryParseDisplayName(Type enumType, string s, out object? result, CultureInfo? cultureInfo = null)
    {
        TestIsEnumType(enumType);

        foreach (var enumName in enumType.GetEnumNames())
        {
            var enumValue = Enum.Parse(enumType, enumName);

            var displayString = EnumValueToDisplayName(enumValue, cultureInfo);

            if (Equals(displayString, s))
            {
                result = enumValue;
                return true;
            }
        }

        result = null;
        return false;
    }

    public static bool TryParseDisplayName<TEnum>(string s, out TEnum? result, CultureInfo? cultureInfo = null)
        where TEnum : Enum
    {
        var functionResult = TryParseDisplayName(typeof(TEnum), s, out object? dataResult, cultureInfo);
        if (functionResult)
        {
            result = (TEnum?)dataResult;
            return true;
        }

        result = default;
        return false;
    }
}