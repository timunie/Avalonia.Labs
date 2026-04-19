using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
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
[TemplatePart("PART_DropDownHeader",           typeof(ContentPresenter))]
[TemplatePart("PART_DropDownFooter",           typeof(ContentPresenter))]
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

    // ── Timer for SelectItemsFromTextInputDelay ───────────────────────
    private DispatcherTimer? _textInputTimer;

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

    /// <summary>Defines the <see cref="HasCustomText"/> property.</summary>
    public static readonly DirectProperty<MultiSelectionComboBox, bool> HasCustomTextProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectionComboBox, bool>(
            nameof(HasCustomText), o => o.HasCustomText);

    private bool _hasCustomText;

    /// <summary>
    /// Gets whether <see cref="Text"/> differs from the computed joined selected-items text.
    /// Recomputed on both Text change and selection change.
    /// </summary>
    public bool HasCustomText
    {
        get => _hasCustomText;
        private set => SetAndRaise(HasCustomTextProperty, ref _hasCustomText, value);
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

    /// <summary>Defines the <see cref="OrderSelectedItemsBy"/> property.</summary>
    public static readonly StyledProperty<SelectedItemsOrderType> OrderSelectedItemsByProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, SelectedItemsOrderType>(nameof(OrderSelectedItemsBy), SelectedItemsOrderType.SelectedOrder);

    /// <summary>Defines the <see cref="SelectedItemStringFormat"/> property.</summary>
    public static readonly StyledProperty<string?> SelectedItemStringFormatProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, string?>(nameof(SelectedItemStringFormat));

    /// <summary>Defines the <see cref="ObjectToStringComparer"/> property.</summary>
    public static readonly StyledProperty<ICompareObjectToString?> ObjectToStringComparerProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, ICompareObjectToString?>(nameof(ObjectToStringComparer));

    /// <summary>Defines the <see cref="EditableTextStringComparision"/> property.</summary>
    public static readonly StyledProperty<StringComparison> EditableTextStringComparisionProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, StringComparison>(nameof(EditableTextStringComparision), StringComparison.Ordinal);

    /// <summary>Defines the <see cref="StringToObjectParser"/> property.</summary>
    public static readonly StyledProperty<IParseStringToObject?> StringToObjectParserProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, IParseStringToObject?>(nameof(StringToObjectParser));

    /// <summary>Defines the <see cref="SelectItemsFromTextInputDelay"/> property.</summary>
    public static readonly StyledProperty<int> SelectItemsFromTextInputDelayProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, int>(nameof(SelectItemsFromTextInputDelay), -1);

    /// <summary>Defines the <see cref="IsDropDownHeaderVisible"/> property.</summary>
    public static readonly StyledProperty<bool> IsDropDownHeaderVisibleProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, bool>(nameof(IsDropDownHeaderVisible));

    /// <summary>Defines the <see cref="DropDownHeaderContent"/> property.</summary>
    public static readonly StyledProperty<object?> DropDownHeaderContentProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, object?>(nameof(DropDownHeaderContent));

    /// <summary>Defines the <see cref="DropDownHeaderContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> DropDownHeaderContentTemplateProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, IDataTemplate?>(nameof(DropDownHeaderContentTemplate));

    /// <summary>Defines the <see cref="IsDropDownFooterVisible"/> property.</summary>
    public static readonly StyledProperty<bool> IsDropDownFooterVisibleProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, bool>(nameof(IsDropDownFooterVisible));

    /// <summary>Defines the <see cref="DropDownFooterContent"/> property.</summary>
    public static readonly StyledProperty<object?> DropDownFooterContentProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, object?>(nameof(DropDownFooterContent));

    /// <summary>Defines the <see cref="DropDownFooterContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> DropDownFooterContentTemplateProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, IDataTemplate?>(nameof(DropDownFooterContentTemplate));

    /// <summary>Defines the <see cref="InterceptKeyboardSelection"/> property.</summary>
    public static readonly StyledProperty<bool> InterceptKeyboardSelectionProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, bool>(nameof(InterceptKeyboardSelection), true);

    /// <summary>Defines the <see cref="InterceptMouseWheelSelection"/> property.</summary>
    public static readonly StyledProperty<bool> InterceptMouseWheelSelectionProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, bool>(nameof(InterceptMouseWheelSelection), true);

    /// <summary>Defines the <see cref="TextWrapping"/> property.</summary>
    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);

    /// <summary>Defines the <see cref="AcceptsReturn"/> property.</summary>
    public static readonly StyledProperty<bool> AcceptsReturnProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, bool>(nameof(AcceptsReturn));

    /// <summary>Defines the <see cref="SelectedItemsPanelTemplate"/> property.</summary>
    public static readonly StyledProperty<ITemplate<Panel?>?> SelectedItemsPanelTemplateProperty =
        AvaloniaProperty.Register<MultiSelectionComboBox, ITemplate<Panel?>?>(nameof(SelectedItemsPanelTemplate));

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

    /// <summary>Gets or sets how selected items are ordered in the chip strip.</summary>
    public SelectedItemsOrderType OrderSelectedItemsBy
    {
        get => GetValue(OrderSelectedItemsByProperty);
        set => SetValue(OrderSelectedItemsByProperty, value);
    }

    /// <summary>Gets or sets a format string used when converting items to display text.</summary>
    public string? SelectedItemStringFormat
    {
        get => GetValue(SelectedItemStringFormatProperty);
        set => SetValue(SelectedItemStringFormatProperty, value);
    }

    /// <summary>Gets or sets a custom comparer for matching text input to items.</summary>
    public ICompareObjectToString? ObjectToStringComparer
    {
        get => GetValue(ObjectToStringComparerProperty);
        set => SetValue(ObjectToStringComparerProperty, value);
    }

    /// <summary>Gets or sets the <see cref="StringComparison"/> used when matching text input to items.</summary>
    public StringComparison EditableTextStringComparision
    {
        get => GetValue(EditableTextStringComparisionProperty);
        set => SetValue(EditableTextStringComparisionProperty, value);
    }

    /// <summary>Gets or sets a parser that creates new items from typed text.</summary>
    public IParseStringToObject? StringToObjectParser
    {
        get => GetValue(StringToObjectParserProperty);
        set => SetValue(StringToObjectParserProperty, value);
    }

    /// <summary>
    /// Gets or sets a delay in milliseconds before auto-selecting items from typed text.
    /// -1 (the default) disables auto-selection.
    /// </summary>
    public int SelectItemsFromTextInputDelay
    {
        get => GetValue(SelectItemsFromTextInputDelayProperty);
        set => SetValue(SelectItemsFromTextInputDelayProperty, value);
    }

    /// <summary>Gets or sets whether the drop-down header area is visible.</summary>
    public bool IsDropDownHeaderVisible
    {
        get => GetValue(IsDropDownHeaderVisibleProperty);
        set => SetValue(IsDropDownHeaderVisibleProperty, value);
    }

    /// <summary>Gets or sets the content shown in the drop-down header.</summary>
    public object? DropDownHeaderContent
    {
        get => GetValue(DropDownHeaderContentProperty);
        set => SetValue(DropDownHeaderContentProperty, value);
    }

    /// <summary>Gets or sets the data template for the drop-down header content.</summary>
    public IDataTemplate? DropDownHeaderContentTemplate
    {
        get => GetValue(DropDownHeaderContentTemplateProperty);
        set => SetValue(DropDownHeaderContentTemplateProperty, value);
    }

    /// <summary>Gets or sets whether the drop-down footer area is visible.</summary>
    public bool IsDropDownFooterVisible
    {
        get => GetValue(IsDropDownFooterVisibleProperty);
        set => SetValue(IsDropDownFooterVisibleProperty, value);
    }

    /// <summary>Gets or sets the content shown in the drop-down footer.</summary>
    public object? DropDownFooterContent
    {
        get => GetValue(DropDownFooterContentProperty);
        set => SetValue(DropDownFooterContentProperty, value);
    }

    /// <summary>Gets or sets the data template for the drop-down footer content.</summary>
    public IDataTemplate? DropDownFooterContentTemplate
    {
        get => GetValue(DropDownFooterContentTemplateProperty);
        set => SetValue(DropDownFooterContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets whether Up/Down arrow keys open the drop-down in single-select mode.
    /// When <see langword="false"/>, arrow keys do not change selection.
    /// </summary>
    public bool InterceptKeyboardSelection
    {
        get => GetValue(InterceptKeyboardSelectionProperty);
        set => SetValue(InterceptKeyboardSelectionProperty, value);
    }

    /// <summary>
    /// Gets or sets whether mouse wheel scrolling changes selection in single-select mode.
    /// </summary>
    public bool InterceptMouseWheelSelection
    {
        get => GetValue(InterceptMouseWheelSelectionProperty);
        set => SetValue(InterceptMouseWheelSelectionProperty, value);
    }

    /// <summary>Gets or sets the text wrapping behavior for the editable TextBox.</summary>
    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    /// <summary>Gets or sets whether the editable TextBox accepts the Return key.</summary>
    public bool AcceptsReturn
    {
        get => GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }

    /// <summary>Gets or sets the panel template for the chip ItemsControl.</summary>
    public ITemplate<Panel?>? SelectedItemsPanelTemplate
    {
        get => GetValue(SelectedItemsPanelTemplateProperty);
        set => SetValue(SelectedItemsPanelTemplateProperty, value);
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
    // Routed events
    // ══════════════════════════════════════════════════════════════════

    public static readonly RoutedEvent<AddingItemEventArgs> AddingItemEvent =
        RoutedEvent.Register<MultiSelectionComboBox, AddingItemEventArgs>(nameof(AddingItem), RoutingStrategies.Bubble);

    public event EventHandler<AddingItemEventArgs> AddingItem
    {
        add => AddHandler(AddingItemEvent, value);
        remove => RemoveHandler(AddingItemEvent, value);
    }

    public static readonly RoutedEvent<AddedItemEventArgs> AddedItemEvent =
        RoutedEvent.Register<MultiSelectionComboBox, AddedItemEventArgs>(nameof(AddedItem), RoutingStrategies.Bubble);

    public event EventHandler<AddedItemEventArgs> AddedItem
    {
        add => AddHandler(AddedItemEvent, value);
        remove => RemoveHandler(AddedItemEvent, value);
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

        SelectItemsFromTextInputDelayProperty.Changed.AddClassHandler<MultiSelectionComboBox>(
            (s, _) => s.StopTextInputTimer());
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

        StopTextInputTimer();

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

        IEnumerable<object> items = SelectedItems?.Cast<object>() ?? [];
        if (OrderSelectedItemsBy == SelectedItemsOrderType.ItemsSourceOrder)
        {
            items = items.OrderBy(item => Items.IndexOf(item));
        }
        DisplaySelectedItems = items.ToList();

        _isUpdatingText = true;
        var joined = string.Join(Separator,
            ((IList<object>)DisplaySelectedItems!).Select(GetDisplayText));
        SetCurrentValue(TextProperty, joined);
        _isUpdatingText = false;

        UpdateHasCustomText();
    }

    private string BuildSelectedItemsText()
    {
        IEnumerable<object> items = SelectedItems?.Cast<object>() ?? [];
        if (OrderSelectedItemsBy == SelectedItemsOrderType.ItemsSourceOrder)
        {
            items = items.OrderBy(item => Items.IndexOf(item));
        }
        return string.Join(Separator, items.Select(GetDisplayText));
    }

    private void UpdateHasCustomText()
    {
        var expected = BuildSelectedItemsText();
        var current  = Text;
        HasCustomText = !string.IsNullOrEmpty(current) && !string.Equals(current, expected, StringComparison.Ordinal);
    }

    // ══════════════════════════════════════════════════════════════════
    // Text / Filter
    // ══════════════════════════════════════════════════════════════════

    private void OnTextChanged()
    {
        // Ignore changes we triggered ourselves (selection → text sync).
        if (_isUpdatingText) return;

        UpdateHasCustomText();

        if (FilterMode != FilterMode.None)
        {
            ApplyFilter(Text);

            if (!string.IsNullOrEmpty(Text) && !IsDropDownOpen)
                SetCurrentValue(IsDropDownOpenProperty, true);
        }

        var delay = SelectItemsFromTextInputDelay;
        if (delay >= 0)
        {
            if (delay == 0)
            {
                UpdateSelectedItemsFromText();
            }
            else
            {
                if (_textInputTimer == null)
                {
                    _textInputTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(delay) };
                    _textInputTimer.Tick += OnTextInputTimerTick;
                }
                else
                {
                    _textInputTimer.Stop();
                    _textInputTimer.Interval = TimeSpan.FromMilliseconds(delay);
                }
                _textInputTimer.Start();
            }
        }
    }

    private void OnTextInputTimerTick(object? sender, EventArgs e)
    {
        StopTextInputTimer();
        UpdateSelectedItemsFromText();
    }

    private void StopTextInputTimer()
    {
        if (_textInputTimer == null) return;
        _textInputTimer.Stop();
        _textInputTimer.Tick -= OnTextInputTimerTick;
        _textInputTimer = null;
    }

    private void UpdateSelectedItemsFromText()
    {
        var text = Text;
        if (string.IsNullOrEmpty(text)) return;

        var isMultiple = (SelectionMode & SelectionMode.Multiple) != 0;
        var tokens = isMultiple ? text.Split(Separator) : [text];

        foreach (var token in tokens)
        {
            var trimmed = token.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            var found = FindItemByText(trimmed);
            if (found != null)
            {
                if (isMultiple)
                {
                    if (SelectedItems?.Contains(found) == false)
                        SelectedItems?.Add(found);
                }
                else
                {
                    SetCurrentValue(SelectedItemProperty, found);
                }
            }
            else if (StringToObjectParser != null && ItemsSource is IList mutableList)
            {
                var parser = StringToObjectParser;
                var targetType = DefaultStringToObjectParser.Instance.GetElementType(mutableList);
                if (parser.TryCreateObjectFromString(trimmed, out var newObj, null, SelectedItemStringFormat, targetType))
                {
                    var args = new AddingItemEventArgs(
                        AddingItemEvent, trimmed, newObj, true,
                        mutableList, targetType, SelectedItemStringFormat, null, parser);
                    RaiseEvent(args);

                    if (args.Accepted)
                    {
                        var itemToAdd = args.ParsedObject;
                        mutableList.Add(itemToAdd);
                        RaiseEvent(new AddedItemEventArgs(AddedItemEvent, itemToAdd, mutableList));

                        if (isMultiple)
                            SelectedItems?.Add(itemToAdd);
                        else
                            SetCurrentValue(SelectedItemProperty, itemToAdd);
                    }
                }
            }
        }
    }

    private object? FindItemByText(string text)
    {
        var comparer   = ObjectToStringComparer;
        var comparison = EditableTextStringComparision;
        var fmt        = SelectedItemStringFormat;

        foreach (var item in Items)
        {
            if (comparer != null)
            {
                if (comparer.CheckIfStringMatchesObject(text, item, comparison, fmt))
                    return item;
            }
            else
            {
                if (string.Equals(text, GetDisplayText(item), comparison))
                    return item;
            }
        }
        return null;
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
    protected virtual string GetDisplayText(object? item)
    {
        var fmt = SelectedItemStringFormat;
        if (item is null) return string.Empty;
        if (string.IsNullOrEmpty(fmt)) return item.ToString() ?? string.Empty;
        if (fmt.Contains('{') && fmt.Contains('}')) return string.Format(fmt, item);
        return string.Format($"{{0:{fmt}}}", item);
    }

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
        switch (e.Key)
        {
            case Key.Escape when IsDropDownOpen:
                SetCurrentValue(IsDropDownOpenProperty, false);
                e.Handled = true;
                break;

            case Key.A when e.KeyModifiers.HasFlag(KeyModifiers.Control)
                         && (SelectionMode & SelectionMode.Multiple) != 0:
                SelectAll();
                e.Handled = true;
                break;

            case Key.F4:
            case Key.Down when !IsDropDownOpen:
            case Key.Up   when !IsDropDownOpen:
                if (e.Key != Key.F4 && !InterceptKeyboardSelection
                    && (SelectionMode & SelectionMode.Multiple) == 0)
                    break; // skip, let it bubble
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

        if (!e.Handled) base.OnKeyDown(e);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (InterceptMouseWheelSelection && !IsDropDownOpen && (SelectionMode & SelectionMode.Multiple) == 0)
        {
            var delta = e.Delta.Y;
            var idx   = SelectedIndex;
            if (delta < 0 && idx < Items.Count - 1) SelectedIndex = idx + 1;
            else if (delta > 0 && idx > 0)           SelectedIndex = idx - 1;
            e.Handled = true;
            return;
        }
        base.OnPointerWheelChanged(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopTextInputTimer();
        base.OnDetachedFromVisualTree(e);
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
