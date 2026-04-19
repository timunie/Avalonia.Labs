using System;
using System.Globalization;

namespace Avalonia.Labs.Controls;

public interface IParseStringToObject
{
    bool TryCreateObjectFromString(string? input, out object? result, CultureInfo? culture = null, string? stringFormat = null, Type? targetType = null);
}
