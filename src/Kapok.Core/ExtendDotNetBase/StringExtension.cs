using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Res = Kapok.Resources.ExtendDotNetBase.StringExtension;

// ReSharper disable once CheckNamespace
namespace System;

public static class StringExtension
{
    public static string? Truncate(this string? value, [Range(0, int.MaxValue)] int maxLength)
    {
        if (value == null) return null;
        if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength), Res.Truncate_ParameterLowerThanZero);

        return value.Length <= maxLength
            ? value
            : value.Substring(0, maxLength);
    }

    public static bool TryParseToValueType(this string value, Type type, out object result,
        NumberStyles numberStyles = default, DateTimeStyles dateTimeStyles = default, IFormatProvider? provider = default)
    {
        bool returnValue;

        if (type == typeof(bool))
        {
            returnValue = bool.TryParse(value, out bool boolResult);
            result = boolResult;
        }
        else if (type == typeof(byte))
        {
            returnValue = byte.TryParse(value, numberStyles, provider, out byte byteResult);
            result = byteResult;
        }
        else if (type == typeof(char))
        {
            returnValue = char.TryParse(value, out char charResult);
            result = charResult;
        }
        else if (type == typeof(decimal))
        {
            returnValue = decimal.TryParse(value, numberStyles, provider, out decimal decimalResult);
            result = decimalResult;
        }
        else if (type == typeof(double))
        {
            returnValue = double.TryParse(value, numberStyles, provider, out double doubleResult);
            result = doubleResult;
        }
        else if (type == typeof(float))
        {
            returnValue = float.TryParse(value, numberStyles, provider, out float floatResult);
            result = floatResult;
        }
        else if (type == typeof(int))
        {
            returnValue = int.TryParse(value, numberStyles, provider, out int intResult);
            result = intResult;
        }
        else if (type == typeof(long))
        {
            returnValue = long.TryParse(value, numberStyles, provider, out long longResult);
            result = longResult;
        }
        else if (type == typeof(sbyte))
        {
            returnValue = sbyte.TryParse(value, numberStyles, provider, out sbyte sbyteResult);
            result = sbyteResult;
        }
        else if (type == typeof(short))
        {
            returnValue = short.TryParse(value, numberStyles, provider, out short shortResult);
            result = shortResult;
        }
        else if (type == typeof(uint))
        {
            returnValue = uint.TryParse(value, numberStyles, provider, out uint uintResult);
            result = uintResult;
        }
        else if (type == typeof(ulong))
        {
            returnValue = ulong.TryParse(value, numberStyles, provider, out ulong ulongResult);
            result = ulongResult;
        }
        else if (type == typeof(ushort))
        {
            returnValue = ushort.TryParse(value, numberStyles, provider, out ushort ushortResult);
            result = ushortResult;
        }
        else if (type == typeof(DateTime))
        {
            returnValue = DateTime.TryParse(value, provider, dateTimeStyles, out DateTime dateTimeResult);
            result = dateTimeResult;
        }
        else if (type == typeof(TimeSpan))
        {
            returnValue = TimeSpan.TryParse(value, provider, out TimeSpan timeSpanResult);
            result = timeSpanResult;
        }
        else if (type == typeof(Guid))
        {
            returnValue = Guid.TryParse(value, out Guid guidResult);
            result = guidResult;
        }
        else
        {
            throw new ArgumentException($"The type {type.FullName} is not supported by this function.");
        }

        return returnValue;
    }
}