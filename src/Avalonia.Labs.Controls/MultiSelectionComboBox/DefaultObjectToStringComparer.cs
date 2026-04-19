using System;

namespace Avalonia.Labs.Controls;

/// <summary>
/// Default: formats object (using string.Format with stringFormat if provided, else ToString()),
/// then compares with input using the given StringComparison.
/// </summary>
public sealed class DefaultObjectToStringComparer : ICompareObjectToString
{
    public static readonly DefaultObjectToStringComparer Instance = new();

    public bool CheckIfStringMatchesObject(string? input, object? objectToCompare, StringComparison stringComparison, string? stringFormat)
    {
        if (input is null) return objectToCompare is null;
        if (objectToCompare is null) return false;

        string objectText;
        if (string.IsNullOrEmpty(stringFormat))
            objectText = objectToCompare.ToString() ?? string.Empty;
        else if (stringFormat!.Contains('{') && stringFormat.Contains('}'))
            objectText = string.Format(stringFormat, objectToCompare);
        else
            objectText = string.Format($"{{0:{stringFormat}}}", objectToCompare);

        return input.Equals(objectText, stringComparison);
    }
}
