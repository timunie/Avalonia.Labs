using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;

namespace Avalonia.Labs.Controls;

/// <summary>
/// An item container used in the drop-down list of <see cref="MultiSelectionComboBox"/>.
/// </summary>
[PseudoClasses(":pressed", ":selected")]
public class MultiSelectionComboBoxItem : ListBoxItem
{
    static MultiSelectionComboBoxItem()
    {
        PressedMixin.Attach<MultiSelectionComboBoxItem>();
        FocusableProperty.OverrideDefaultValue<MultiSelectionComboBoxItem>(true);
    }
}
