namespace Avalonia.Labs.Controls;

/// <summary>
/// Controls how the typed text in an editable <see cref="MultiSelectionComboBox"/> filters
/// the items shown in the drop-down.
/// </summary>
public enum FilterMode
{
    /// <summary>No filtering. All items are always visible.</summary>
    None,
    /// <summary>Only items whose display text starts with the typed text are shown.</summary>
    StartsWith,
    /// <summary>Only items whose display text contains the typed text are shown.</summary>
    Contains,
}
