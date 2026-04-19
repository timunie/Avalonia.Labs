using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Avalonia.Labs.Controls;

/// <summary>Uses TypeConverter to convert strings to typed objects.</summary>
public sealed class DefaultStringToObjectParser : IParseStringToObject
{
    public static readonly DefaultStringToObjectParser Instance = new();

    public bool TryCreateObjectFromString(string? input, out object? result, CultureInfo? culture = null, string? stringFormat = null, Type? targetType = null)
    {
        try
        {
            if (input is null) { result = null; return true; }
            if (targetType is null) { result = null; return false; }
#pragma warning disable IL2026, IL2067
            result = TypeDescriptor.GetConverter(targetType).ConvertFromString(null!, culture ?? CultureInfo.InvariantCulture, input);
#pragma warning restore IL2026, IL2067
            return true;
        }
        catch { result = null; return false; }
    }

    public Type? GetElementType(IEnumerable? list)
    {
        if (list is null) return null;
        var t = list.GetType();
        return t.IsGenericType ? t.GetGenericArguments().FirstOrDefault() : t.GetElementType();
    }
}
