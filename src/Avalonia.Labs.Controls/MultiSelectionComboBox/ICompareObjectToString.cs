using System;

namespace Avalonia.Labs.Controls;

/// <summary>
/// Used to check if a given string represents an item object.
/// Provides a custom object-to-string comparison strategy.
/// </summary>
public interface ICompareObjectToString
{
    bool CheckIfStringMatchesObject(string? input, object? objectToCompare, StringComparison stringComparison, string? stringFormat);
}
