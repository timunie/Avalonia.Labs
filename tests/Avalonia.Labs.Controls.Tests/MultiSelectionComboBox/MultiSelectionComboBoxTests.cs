using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Xunit;

namespace Avalonia.Labs.Controls.Tests.MultiSelectionComboBox;

public class MultiSelectionComboBoxTests
{
    private static async Task<Controls.MultiSelectionComboBox> CreateLoadedAsync(
        Action<Controls.MultiSelectionComboBox>? configure = null)
    {
        var (mscb, _) = await CreateLoadedWithWindowAsync(configure);
        return mscb;
    }

    private static async Task<(Controls.MultiSelectionComboBox Mscb, Window Window)> CreateLoadedWithWindowAsync(
        Action<Controls.MultiSelectionComboBox>? configure = null)
    {
        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
        };
        configure?.Invoke(mscb);

        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();

        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        return (mscb, window);
    }

    // ─── HasCustomText ───────────────────────────────────────────────────────

    [AvaloniaFact]
    public async Task HasCustomText_IsFlase_WhenTextIsNullAndItemsSelected()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
        });

        mscb.Selection.Select(0);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.False(mscb.HasCustomText);
    }

    [AvaloniaFact]
    public async Task HasCustomText_IsFalse_WhenTextIsEmptyAndItemsSelected()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
        });

        mscb.Selection.Select(0);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.Text = "";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.False(mscb.HasCustomText);
    }

    [AvaloniaFact]
    public async Task HasCustomText_IsFalse_WhenTextMatchesSelectedItemsText()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        mscb.Selection.Select(0);
        mscb.Selection.Select(1);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Set Text to exactly what the selected items produce
        var itemsText = mscb.GetSelectedItemsText();
        mscb.Text = itemsText;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.False(mscb.HasCustomText);
    }

    [AvaloniaFact]
    public async Task HasCustomText_IsTrue_WhenTextDiffersFromSelectedItemsText()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        mscb.Selection.Select(0);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.Text = "something custom";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.True(mscb.HasCustomText);
    }

    [AvaloniaFact]
    public async Task HasCustomText_BecomesFlase_WhenTextClearedAfterTyping()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple" };
        });

        mscb.Selection.Select(0);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.Text = "custom text";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.True(mscb.HasCustomText);

        mscb.Text = "";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.False(mscb.HasCustomText);
    }

    // ─── Pseudoclasses ───────────────────────────────────────────────────────

    [AvaloniaFact]
    public async Task Pseudoclass_HasCustomText_TracksHasCustomText()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple" };
        });

        mscb.Selection.Select(0);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.DoesNotContain(":has-custom-text", mscb.Classes);

        mscb.Text = "different text";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Contains(":has-custom-text", mscb.Classes);

        mscb.Text = "";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.DoesNotContain(":has-custom-text", mscb.Classes);
    }

    [AvaloniaFact]
    public async Task Pseudoclass_Multiple_TracksSelectionMode()
    {
        var mscb = await CreateLoadedAsync();

        Assert.Contains(":multiple", mscb.Classes);

        mscb.SelectionMode = SelectionMode.Single;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.DoesNotContain(":multiple", mscb.Classes);

        mscb.SelectionMode = SelectionMode.Multiple;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Contains(":multiple", mscb.Classes);
    }

    [AvaloniaFact]
    public async Task Pseudoclass_HasSelections_TracksSelectedItems()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        Assert.DoesNotContain(":has-selections", mscb.Classes);

        mscb.Selection.Select(0);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Contains(":has-selections", mscb.Classes);

        mscb.Selection.Clear();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.DoesNotContain(":has-selections", mscb.Classes);
    }

    [AvaloniaFact]
    public async Task Pseudoclass_Editable_TracksIsEditable()
    {
        var mscb = await CreateLoadedAsync();

        Assert.Contains(":editable", mscb.Classes);

        mscb.IsEditable = false;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.DoesNotContain(":editable", mscb.Classes);

        mscb.IsEditable = true;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Contains(":editable", mscb.Classes);
    }

    // ─── GetSelectedItemsText ────────────────────────────────────────────────

    [AvaloniaFact]
    public async Task GetSelectedItemsText_ReturnsNull_WhenNoItemsSelected()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        var text = mscb.GetSelectedItemsText();
        Assert.Null(text);
    }

    [AvaloniaFact]
    public async Task GetSelectedItemsText_ReturnsSingleItem()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        mscb.Selection.Select(0);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var text = mscb.GetSelectedItemsText();
        Assert.Equal("Apple", text);
    }

    [AvaloniaFact]
    public async Task GetSelectedItemsText_ReturnsJoinedItems_WithSeparator()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        mscb.Selection.Select(0);
        mscb.Selection.Select(1);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var text = mscb.GetSelectedItemsText();
        Assert.Equal("Apple, Banana", text);
    }

    [AvaloniaFact]
    public async Task GetSelectedItemsText_AppliesSelectedItemStringFormat()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
            m.SelectedItemStringFormat = "{0}!";
        });

        mscb.Selection.Select(0);
        mscb.Selection.Select(1);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var text = mscb.GetSelectedItemsText();
        Assert.Equal("Apple!, Banana!", text);
    }

    [AvaloniaFact]
    public async Task GetSelectedItemsText_SingleMode_ReturnsSelectedItem()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.SelectionMode = SelectionMode.Single;
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        mscb.SelectedItem = "Banana";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var text = mscb.GetSelectedItemsText();
        Assert.Equal("Banana", text);
    }

    // ─── RemoveItem ──────────────────────────────────────────────────────────

    [AvaloniaFact]
    public async Task RemoveItem_RemovesFromSelectedItems_InMultipleMode()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        mscb.Selection.Select(0);
        mscb.Selection.Select(1);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal(2, mscb.SelectedItems!.Count);

        mscb.RemoveItem("Apple");
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Single(mscb.SelectedItems!);
        Assert.Equal("Banana", mscb.SelectedItems![0]);
    }

    [AvaloniaFact]
    public async Task RemoveItem_ClearsSelectedItem_InSingleMode()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.SelectionMode = SelectionMode.Single;
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        mscb.SelectedItem = "Apple";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Apple", mscb.SelectedItem);

        mscb.RemoveItem("Apple");
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Null(mscb.SelectedItem);
    }

    [AvaloniaFact]
    public async Task RemoveItem_NullItem_DoesNotThrow()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple" };
        });

        mscb.Selection.Select(0);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var ex = Record.Exception(() => mscb.RemoveItem(null));
        Assert.Null(ex);
        Assert.Single(mscb.SelectedItems!);
    }

    // ─── Clear ───────────────────────────────────────────────────────────────

    [AvaloniaFact]
    public async Task Clear_WithCustomText_ResetsText_KeepsSelections()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        mscb.Selection.Select(0);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.Text = "custom";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.True(mscb.HasCustomText);

        mscb.Clear();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.False(mscb.HasCustomText);
        Assert.Single(mscb.SelectedItems!);
    }

    [AvaloniaFact]
    public async Task Clear_WithoutCustomText_ClearsSelections()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        mscb.Selection.Select(0);
        mscb.Selection.Select(1);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.False(mscb.HasCustomText);
        Assert.Equal(2, mscb.SelectedItems!.Count);

        mscb.Clear();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Empty(mscb.SelectedItems!);
    }

    // ─── ForceItemsSelection ─────────────────────────────────────────────────

    [AvaloniaFact]
    public async Task ForceItemsSelection_SelectsMatchingItems_WithSeparator()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
        });

        mscb.Text = "Apple, Cherry";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.ForceItemsSelection();
        // Timer fires at DispatcherPriority.Background
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var selectedTexts = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Apple", selectedTexts);
        Assert.Contains("Cherry", selectedTexts);
        Assert.DoesNotContain("Banana", selectedTexts);
    }

    // ─── DataContext ordering ─────────────────────────────────────────────────

    [AvaloniaFact]
    public async Task DataContextSwap_PreservesNewVmText_NoStaleHasCustomText()
    {
        var items = new List<string> { "Apple", "Banana" };

        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = items;
        });

        // Simulate a first VM: select Apple
        mscb.Selection.Select(0);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.False(mscb.HasCustomText);

        // Simulate a second VM: no selection, no text — should reset cleanly
        mscb.DataContext = new object();
        mscb.Selection.Clear();
        mscb.Text = null;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.False(mscb.HasCustomText);
        Assert.Empty(mscb.SelectedItems!);
    }

    // ─── DisplaySelectedItems ordering ───────────────────────────────────────

    [AvaloniaFact]
    public async Task GetSelectedItemsText_SelectedOrder_FollowsSelectionOrder()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.OrderSelectedItemsBy = SelectedItemsOrderType.SelectedOrder;
        });

        // Select in reverse order: Cherry first, then Apple
        mscb.Selection.Select(2); // Cherry
        mscb.Selection.Select(0); // Apple
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var text = mscb.GetSelectedItemsText();
        // SelectedOrder = order items were selected: Cherry, Apple
        Assert.Equal("Cherry, Apple", text);
    }

    [AvaloniaFact]
    public async Task GetSelectedItemsText_ItemsSourceOrder_FollowsSourceOrder()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.OrderSelectedItemsBy = SelectedItemsOrderType.ItemsSourceOrder;
        });

        // Select in reverse order: Cherry first, then Apple
        mscb.Selection.Select(2); // Cherry
        mscb.Selection.Select(0); // Apple
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var text = mscb.GetSelectedItemsText();
        // ItemsSourceOrder = source order: Apple, Cherry
        Assert.Equal("Apple, Cherry", text);
    }

    // ─── Select by typing ────────────────────────────────────────────────────

    [AvaloniaFact]
    public async Task SelectItemsByText_InSingleMode_SelectsMatchingItem()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.SelectionMode = SelectionMode.Single;
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
        });

        mscb.Text = "Banana";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.ForceItemsSelection();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Banana", mscb.SelectedItem);
    }

    [AvaloniaFact]
    public async Task SelectItemsByText_InSingleMode_UnknownText_DoesNotSelect()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.SelectionMode = SelectionMode.Single;
            m.ItemsSource = new List<string> { "Apple", "Banana" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
        });

        mscb.Text = "Mango";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.ForceItemsSelection();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Null(mscb.SelectedItem);
    }

    // ─── Add items by text ───────────────────────────────────────────────────

    [AvaloniaFact]
    public async Task AddItem_WhenStringToObjectParserIsSet_AddsNewItemToSource()
    {
        var items = new ObservableCollection<string> { "Apple", "Banana" };
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = items;
            m.StringToObjectParser = DefaultStringToObjectParser.Instance;
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
        });

        mscb.Text = "Mango";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.ForceItemsSelection();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Contains("Mango", items);
    }

    [AvaloniaFact]
    public async Task AddItem_WhenHandlerRejectsItem_DoesNotAddToSource()
    {
        var items = new ObservableCollection<string> { "Apple", "Banana" };
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = items;
            m.StringToObjectParser = DefaultStringToObjectParser.Instance;
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
        });

        mscb.AddingItem += (_, e) => e.Handled = true; // reject all new items
        mscb.Text = "Mango";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.ForceItemsSelection();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.DoesNotContain("Mango", items);
    }

    // ─── Enter key confirmation ───────────────────────────────────────────────

    [AvaloniaFact]
    public async Task EnterKey_SelectsItemFromTypedText()
    {
        var (mscb, window) = await CreateLoadedWithWindowAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
        });

        mscb.Focus();
        mscb.Text = "Banana";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var selectedTexts = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Banana", selectedTexts);
    }

    [AvaloniaFact]
    public async Task EnterKey_InSingleMode_SelectsItem()
    {
        var (mscb, window) = await CreateLoadedWithWindowAsync(m =>
        {
            m.SelectionMode = SelectionMode.Single;
            m.ItemsSource = new List<string> { "Apple", "Banana" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
        });

        mscb.Focus();
        mscb.Text = "Apple";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Apple", mscb.SelectedItem);
    }

    // ─── SelectItemsFromTextInputDelay (auto-select while typing) ────────────

    [AvaloniaFact]
    public async Task TypingText_WithAutoDelay_SelectsMatchingItem()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
            m.SelectItemsFromTextInputDelay = 0;
        });

        mscb.Text = "Banana";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var selectedTexts = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Banana", selectedTexts);
    }

    [AvaloniaFact]
    public async Task TypingText_WithAutoDelay_SelectsMultipleMatchingItems()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
            m.SelectItemsFromTextInputDelay = 0;
        });

        mscb.Text = "Apple, Cherry";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var selectedTexts = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Apple", selectedTexts);
        Assert.Contains("Cherry", selectedTexts);
        Assert.DoesNotContain("Banana", selectedTexts);
    }

    [AvaloniaFact]
    public async Task TypingText_WithAutoDelay_ClearsSelection_WhenTextCleared()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
            m.SelectItemsFromTextInputDelay = 0;
        });

        mscb.Text = "Apple";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.NotEmpty(mscb.SelectedItems!);

        mscb.Text = "";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Empty(mscb.SelectedItems!);
    }

    // ─── Auto-add unknown items while typing ─────────────────────────────────

    [AvaloniaFact]
    public async Task TypingUnknownText_WithParserAndAutoDelay_AddsItemToSourceAndSelectsIt()
    {
        var items = new ObservableCollection<string> { "Apple", "Banana" };
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = items;
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
            m.StringToObjectParser = DefaultStringToObjectParser.Instance;
            m.SelectItemsFromTextInputDelay = 0;
        });

        mscb.Text = "Mango";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Contains("Mango", items);
        var selectedTexts = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Mango", selectedTexts);
    }

    [AvaloniaFact]
    public async Task TypingUnknownText_WithoutParser_DoesNotAddItemToSource()
    {
        var items = new ObservableCollection<string> { "Apple", "Banana" };
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = items;
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
            // No StringToObjectParser set
            m.SelectItemsFromTextInputDelay = 0;
        });

        mscb.Text = "Mango";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.DoesNotContain("Mango", items);
        Assert.Empty(mscb.SelectedItems!);
    }

    [AvaloniaFact]
    public async Task TypingUnknownText_WithParserAndAutoDelay_AddingItemEventCanRejectIt()
    {
        var items = new ObservableCollection<string> { "Apple", "Banana" };
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = items;
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
            m.StringToObjectParser = DefaultStringToObjectParser.Instance;
            m.SelectItemsFromTextInputDelay = 0;
        });

        mscb.AddingItem += (_, e) => e.Handled = true; // reject all new items

        mscb.Text = "Mango";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.DoesNotContain("Mango", items);
        Assert.Empty(mscb.SelectedItems!);
    }

    // ─── SelectedItems binding ────────────────────────────────────────────────

    [AvaloniaFact]
    public async Task BoundSelectedItems_ReceivesItemsWhenSelected()
    {
        var boundItems = new ObservableCollection<object>();
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.SelectedItems = boundItems;
        });

        mscb.Selection.Select(0);
        mscb.Selection.Select(2);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Contains("Apple", boundItems);
        Assert.Contains("Cherry", boundItems);
        Assert.DoesNotContain("Banana", boundItems);
    }

    [AvaloniaFact]
    public async Task BoundSelectedItems_PrePopulated_SelectsMatchingItems()
    {
        var boundItems = new ObservableCollection<object> { "Banana" };
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.SelectedItems = boundItems;
        });

        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Contains("Banana", mscb.SelectedItems!.Cast<object>());
        Assert.Single(mscb.SelectedItems!);
    }

    [AvaloniaFact]
    public async Task BoundSelectedItems_RemovingFromCollection_DeselectedInMscb()
    {
        var boundItems = new ObservableCollection<object>();
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
            m.SelectedItems = boundItems;
        });

        mscb.Selection.Select(0);
        mscb.Selection.Select(1);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal(2, boundItems.Count);

        boundItems.Remove("Apple");
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Single(mscb.SelectedItems!);
        Assert.Equal("Banana", mscb.SelectedItems![0]);
    }

    [AvaloniaFact]
    public async Task BoundSelectedItems_ReplacingCollection_UpdatesSelection()
    {
        var firstItems = new ObservableCollection<object> { "Apple" };
        var secondItems = new ObservableCollection<object> { "Cherry" };

        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.SelectedItems = firstItems;
        });

        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.Contains("Apple", mscb.SelectedItems!.Cast<object>());

        mscb.SelectedItems = secondItems;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.DoesNotContain("Apple", mscb.SelectedItems!.Cast<object>());
        Assert.Contains("Cherry", mscb.SelectedItems!.Cast<object>());
    }
}
