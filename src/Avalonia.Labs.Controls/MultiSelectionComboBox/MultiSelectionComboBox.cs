using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace Avalonia.Labs.Controls;

/// <summary>
/// A combo box that supports selecting multiple items and displays them as removable chips
/// (in non-editable mode) or as joined text (in editable mode).
/// </summary>
[PseudoClasses(":has-selections", ":multiple", ":editable")]
[TemplatePart("PART_Popup",                   typeof(Popup))]
[TemplatePart("PART_EditableTextBox",          typeof(TextBox))]
[TemplatePart("PART_SelectedItemsPresenter",   typeof(ItemsControl))]
[TemplatePart("PART_DropDownOverlay",          typeof(Border))]
[TemplatePart("PART_ClearButton",              typeof(Button))]
public class MultiSelectionComboBox : ListBox
{
    // ── Pseudoclass names ────────────────────────────────────────────
    private const string PcHasSelections = ":has-selections";
    private const string PcMultiple      = ":multiple";

    // ── Template parts ───────────────────────────────────────────────
    private Popup?       _popup;
    private TextBox?     _editableTextBox;
    private Border?      _dropDownOverlay;
    private Button?      _clearButton;

    // ── Reentrancy guards ────────────────────────────────────────────
    private bool _isUpdatingText;
    private bool _isEnforcingMax;

    // ══════════════════════════════════════════════════════════════════
    // Direct properties
    // ══════════════════════════════════════════════════════════════════

    /// <summary>Defines the <see cref="HasSelections"/> property.</summary>
    public static readonly DirectProperty<MultiSelectionComboBox, bool> HasSelectionsProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectionComboBox, bool>(
            nameof(HasSelections), o => o.HasSelections);

    private bool _hasSelections;

    /// <summary>
    /// Gets whether at least one item is currently selected.
    /// Also drives the <c>:has-selections</c> pseudoclass.
    /// </summary>
    public bool HasSelections
    {
        get => _hasSelections;
        private set
        {
            if (SetAndRaise(HasSelectionsProperty, ref _hasSelections, value))
                PseudoClasses.Set(PcHasSelections, value);
        }
    }

    /// <summary>Defines the <see cref="DisplaySelectedItems"/> property.</summary>
    public static readonly DirectProperty<MultiSelectionComboBox, IEnumerable?> DisplaySelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectionComboBox, IEnumerable?>(
            nameof(DisplaySelectedItems), o => o.DisplaySelectedItems);

    private IEnumerable? _displaySelectedItems;

    /// <summary>
    /// A snapshot of the currently selected items used to populate the chip strip.
    /// Updated automatically when selection changes.
    /// </summary>
    public IEnumerable? DisplaySelectedItems
    {
        get => _displaySelectedItems;
        private set => SetAndRaise(DisplaySelectedItemsProperty, ref _displaySelectedItems, value);
    }

    // ══════════════════════════════════════════════════════════════════
    // Styled properties
    // ══════════════════════════════════════════════════════════════════

    /// <summary>Defines the <see cref="IsEditable"/> property.</summary>
    public static readonly StyledProperty<bool> IsEditableProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, bool>(nameof(IsEditable));

    /// <summary>Defines the <see cref="IsDropDownOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, bool>(nameof(IsDropDownOpen));

    /// <summary>Defines the <see cref="MaxDropDownHeight"/> property.</summary>
    public static readonly StyledProperty<double> MaxDropDownHeightProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, double>(nameof(MaxDropDownHeight), 504d);

    /// <summary>Defines the <see cref="Text"/> property.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, string?>(nameof(Text));

    /// <summary>Defines the <see cref="FilterMode"/> property.</summary>
    public static readonly StyledProperty<FilterMode> FilterModeProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, FilterMode>(nameof(FilterMode));

    /// <summary>Defines the <see cref="PlaceholderText"/> property.</summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, string?>(nameof(PlaceholderText));

    /// <summary>Defines the <see cref="PlaceholderForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, IBrush?>(nameof(PlaceholderForeground));

    /// <summary>Defines the <see cref="Separator"/> property.</summary>
    public static readonly StyledProperty<string> SeparatorProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, string>(nameof(Separator), ", ");

    /// <summary>Defines the <see cref="IsReadOnly"/> property.</summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        TextBox.IsReadOnlyProperty.AddOwner<MultiSelectionComboBox>();

    /// <summary>Defines the <see cref="ShowClearButton"/> property.</summary>
    public static readonly StyledProperty<bool> ShowClearButtonProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, bool>(nameof(ShowClearButton));

    /// <summary>Defines the <see cref="MaxSelectedItems"/> property.</summary>
    public static readonly StyledProperty<int> MaxSelectedItemsProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, int>(nameof(MaxSelectedItems), -1);

    /// <summary>Defines the <see cref="SelectedItemTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> SelectedItemTemplateProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, IDataTemplate?>(nameof(SelectedItemTemplate));

    // ══════════════════════════════════════════════════════════════════
    // Property accessors
    // ══════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets whether the control shows an editable <see cref="TextBox"/>.</summary>
    public bool IsEditable
    {
        get => GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }

    /// <summary>Gets or sets whether the drop-down is open.</summary>
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>Gets or sets the maximum height of the drop-down list.</summary>
    public double MaxDropDownHeight
    {
        get => GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the text shown in the editable TextBox.
    /// <para>
    /// When <see cref="FilterMode"/> is <see cref="FilterMode.None"/> this is kept in sync with
    /// the joined display text of all selected items.  When <see cref="FilterMode"/> is not
    /// <see cref="FilterMode.None"/> this reflects whatever the user is typing.
    /// </para>
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets how items in the drop-down are filtered while the user types.
    /// Filtering is only active when <see cref="IsEditable"/> is <see langword="true"/>.
    /// </summary>
    public FilterMode FilterMode
    {
        get => GetValue(FilterModeProperty);
        set => SetValue(FilterModeProperty, value);
    }

    /// <summary>Gets or sets the placeholder shown when nothing is selected.</summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>Gets or sets the foreground brush of the placeholder text.</summary>
    public IBrush? PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    /// <summary>Gets or sets the string used to join selected items into <see cref="Text"/>.</summary>
    public string Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    /// <summary>Gets or sets whether the editable TextBox is read-only.</summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>Gets or sets whether a clear-all button is shown next to the arrow.</summary>
    public bool ShowClearButton
    {
        get => GetValue(ShowClearButtonProperty);
        set => SetValue(ShowClearButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of items that can be selected simultaneously.
    /// <c>-1</c> (the default) means unlimited.
    /// </summary>
    public int MaxSelectedItems
    {
        get => GetValue(MaxSelectedItemsProperty);
        set => SetValue(MaxSelectedItemsProperty, value);
    }

    /// <summary>Gets or sets the <see cref="IDataTemplate"/> used to display each selected item
    /// in chips (non-editable multiple mode) and in the single-item presenter
    /// (non-editable single mode).  When <see langword="null"/> the item's
    /// <see cref="object.ToString"/> result is displayed.
    /// </summary>
    public IDataTemplate? SelectedItemTemplate
    {
        get => GetValue(SelectedItemTemplateProperty);
        set => SetValue(SelectedItemTemplateProperty, value);
    }

    /// <summary>Defines the <see cref="RemoveSelectedItemCommand"/> property.</summary>
    public static readonly DirectProperty<MultiSelectionComboBox, ICommand?> RemoveSelectedItemCommandProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectionComboBox, ICommand?>(
            nameof(RemoveSelectedItemCommand), o => o.RemoveSelectedItemCommand);

    private ICommand? _removeSelectedItemCommand;

    /// <summary>
    /// Command bound to the ✕ button inside each chip.
    /// The command parameter is the data item to remove.
    /// </summary>
    public ICommand? RemoveSelectedItemCommand
    {
        get => _removeSelectedItemCommand;
        private set => SetAndRaise(RemoveSelectedItemCommandProperty, ref _removeSelectedItemCommand, value);
    }

    // ══════════════════════════════════════════════════════════════════
    // Static constructor – property change handlers
    // ══════════════════════════════════════════════════════════════════

    static MultiSelectionComboBox()
    {
        SelectionModeProperty.Changed.AddClassHandler<MultiSelectionComboBox>(
            (s, _) => s.UpdateMultiplePseudoclass());

        IsDropDownOpenProperty.Changed.AddClassHandler<MultiSelectionComboBox>(
            (s, e) => s.OnIsDropDownOpenChanged(e));

        TextProperty.Changed.AddClassHandler<MultiSelectionComboBox>(
            (s, _) => s.OnTextChanged());

        IsEditableProperty.Changed.AddClassHandler<MultiSelectionComboBox>(
            (s, e) => s.PseudoClasses.Set(":editable", e.GetNewValue<bool>()));
    }

    // ══════════════════════════════════════════════════════════════════
    // Constructor
    // ══════════════════════════════════════════════════════════════════

    public MultiSelectionComboBox()
    {
        RemoveSelectedItemCommand = new LambdaCommand(RemoveSelectedItem);
        SelectionChanged += (_, e) => HandleSelectionChanged(e);
    }

    // ══════════════════════════════════════════════════════════════════
    // Template
    // ══════════════════════════════════════════════════════════════════

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        // Detach old parts
        if (_popup != null)          _popup.Opened          -= OnPopupOpened;
        if (_dropDownOverlay != null) _dropDownOverlay.PointerPressed -= OnDropDownOverlayPointerPressed;
        if (_clearButton != null)     _clearButton.Click     -= OnClearButtonClick;
        if (_editableTextBox != null) _editableTextBox.LostFocus -= OnTextBoxLostFocus;

        base.OnApplyTemplate(e);

        _popup           = e.NameScope.Find<Popup>("PART_Popup");
        _editableTextBox = e.NameScope.Find<TextBox>("PART_EditableTextBox");
        _dropDownOverlay = e.NameScope.Find<Border>("PART_DropDownOverlay");
        _clearButton     = e.NameScope.Find<Button>("PART_ClearButton");

        if (_popup != null)          _popup.Opened          += OnPopupOpened;
        if (_dropDownOverlay != null) _dropDownOverlay.PointerPressed += OnDropDownOverlayPointerPressed;
        if (_clearButton != null)     _clearButton.Click     += OnClearButtonClick;
        if (_editableTextBox != null) _editableTextBox.LostFocus += OnTextBoxLostFocus;

        UpdateMultiplePseudoclass();
        UpdateDisplayAndText();
    }

    // ══════════════════════════════════════════════════════════════════
    // Item containers
    // ══════════════════════════════════════════════════════════════════

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new MultiSelectionComboBoxItem();

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<MultiSelectionComboBoxItem>(item, out recycleKey);

    /// <summary>Applies the current filter to newly realized / recycled containers.</summary>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        container.IsVisible = ItemMatchesFilter(item, Text);
    }

    // ══════════════════════════════════════════════════════════════════
    // Selection
    // ══════════════════════════════════════════════════════════════════

    private void HandleSelectionChanged(SelectionChangedEventArgs e)
    {
        if (_isEnforcingMax) return;

        EnforceMaxSelectedItems(e);
        UpdateDisplayAndText();

        // In single-selection mode close the drop-down when the user picks an item.
        if ((SelectionMode & SelectionMode.Multiple) == 0 && IsDropDownOpen)
            SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void EnforceMaxSelectedItems(SelectionChangedEventArgs e)
    {
        var max = MaxSelectedItems;
        if (max < 0 || SelectedItems == null || SelectedItems.Count <= max) return;

        _isEnforcingMax = true;
        foreach (var item in e.AddedItems.Cast<object>().ToList())
        {
            if (SelectedItems.Count <= max) break;
            SelectedItems.Remove(item);
        }
        _isEnforcingMax = false;
    }

    private void UpdateDisplayAndText()
    {
        HasSelections = SelectedItems?.Count > 0 || SelectedItem != null;
        DisplaySelectedItems = SelectedItems?.Cast<object>().ToList();

        // Keep Text in sync with joined selection (used for display in FilterMode.None and
        // to restore the TextBox after the drop-down closes in filter mode).
        _isUpdatingText = true;
        var joined = string.Join(Separator,
            SelectedItems?.Cast<object>().Select(GetDisplayText) ?? Enumerable.Empty<string>());
        SetCurrentValue(TextProperty, joined);
        _isUpdatingText = false;
    }

    // ══════════════════════════════════════════════════════════════════
    // Text / Filter
    // ══════════════════════════════════════════════════════════════════

    private void OnTextChanged()
    {
        // Ignore changes we triggered ourselves (selection → text sync).
        if (_isUpdatingText) return;

        if (FilterMode != FilterMode.None)
        {
            ApplyFilter(Text);

            if (!string.IsNullOrEmpty(Text) && !IsDropDownOpen)
                SetCurrentValue(IsDropDownOpenProperty, true);
        }
    }

    private void ApplyFilter(string? filterText)
    {
        if (FilterMode == FilterMode.None) return;

        for (var i = 0; i < Items.Count; i++)
        {
            if (ContainerFromIndex(i) is Control c)
                c.IsVisible = ItemMatchesFilter(Items[i], filterText);
        }
    }

    private bool ItemMatchesFilter(object? item, string? filterText)
    {
        if (FilterMode == FilterMode.None || string.IsNullOrEmpty(filterText)) return true;
        var display = GetDisplayText(item);
        return FilterMode switch
        {
            FilterMode.StartsWith => display.StartsWith(filterText, StringComparison.OrdinalIgnoreCase),
            FilterMode.Contains   => display.Contains(filterText,   StringComparison.OrdinalIgnoreCase),
            _                     => true
        };
    }

    /// <summary>
    /// Returns the display string for <paramref name="item"/>.
    /// Override to support custom item types or <c>DisplayMemberBinding</c>-style logic.
    /// </summary>
    protected virtual string GetDisplayText(object? item) => item?.ToString() ?? string.Empty;

    // ══════════════════════════════════════════════════════════════════
    // Drop-down
    // ══════════════════════════════════════════════════════════════════

    private void OnIsDropDownOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<bool>())
        {
            // Re-apply current filter when the drop-down opens (handles stale container state).
            ApplyFilter(Text);
        }
        else
        {
            // Restore Text to the joined selection so the TextBox doesn't look empty after
            // the user typed a filter and then closed the drop-down without selecting anything.
            if (FilterMode != FilterMode.None && IsEditable)
                UpdateDisplayAndText();
        }
    }

    private void OnPopupOpened(object? sender, EventArgs e) => ApplyFilter(Text);

    private void OnDropDownOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
        e.Handled = true;
    }

    private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (!IsKeyboardFocusWithin)
            SetCurrentValue(IsDropDownOpenProperty, false);
    }

    // ══════════════════════════════════════════════════════════════════
    // Clear
    // ══════════════════════════════════════════════════════════════════

    /// <summary>Clears all selected items and resets the text.</summary>
    public void ClearAll()
    {
        SelectedItems?.Clear();
        _isUpdatingText = true;
        SetCurrentValue(TextProperty, string.Empty);
        _isUpdatingText = false;
    }

    /// <summary>Removes a single item from the selection.</summary>
    public void RemoveSelectedItem(object? item)
    {
        if (item != null) SelectedItems?.Remove(item);
    }

    private void OnClearButtonClick(object? sender, RoutedEventArgs e) => ClearAll();

    // ══════════════════════════════════════════════════════════════════
    // Keyboard
    // ══════════════════════════════════════════════════════════════════

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.Key)
        {
            case Key.Escape when IsDropDownOpen:
                SetCurrentValue(IsDropDownOpenProperty, false);
                e.Handled = true;
                break;

            case Key.F4:
            case Key.Down when !IsDropDownOpen:
            case Key.Up   when !IsDropDownOpen:
                SetCurrentValue(IsDropDownOpenProperty, true);
                Dispatcher.UIThread.Post(FocusFirstItem, DispatcherPriority.Loaded);
                e.Handled = true;
                break;

            // Backspace with an empty editable TextBox removes the most recent chip.
            case Key.Back when IsEditable && string.IsNullOrEmpty(_editableTextBox?.Text):
                if (SelectedItems?.Count > 0)
                {
                    SelectedItems.RemoveAt(SelectedItems.Count - 1);
                    e.Handled = true;
                }
                break;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        // In non-editable mode the whole body area toggles the drop-down.
        if (!IsEditable && !e.Handled)
        {
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }
    }

    private void FocusFirstItem()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (ContainerFromIndex(i) is { IsVisible: true, IsEnabled: true } c)
            {
                c.Focus();
                break;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // Pseudoclasses
    // ══════════════════════════════════════════════════════════════════

    private void UpdateMultiplePseudoclass()
        => PseudoClasses.Set(PcMultiple, (SelectionMode & SelectionMode.Multiple) != 0);

    // ══════════════════════════════════════════════════════════════════
    // Helpers
    // ══════════════════════════════════════════════════════════════════

    private sealed class LambdaCommand : ICommand
    {
        private readonly Action<object?> _execute;
        public LambdaCommand(Action<object?> execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? _) => true;
        public void Execute(object? parameter) => _execute(parameter);
    }
}
