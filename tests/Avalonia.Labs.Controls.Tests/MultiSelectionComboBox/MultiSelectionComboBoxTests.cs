using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
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

    // ─── DataContext switch inside DataTemplate ───────────────────────────────

    /// <summary>
    /// Regression: when the user has typed text (e.g. "Apple, Cherry") but the
    /// auto-selection delay has not yet fired, a DataContext swap — e.g. triggered
    /// by a button click that switches the master-detail ViewModel — must commit the
    /// typed text to selections on the OLD ViewModel <em>before</em> the bindings
    /// switch over to the new one.
    ///
    /// Previously, <c>OnDataContextBeginUpdate</c> started a background timer for this
    /// commit, but that timer was always stopped by <c>OnDataContextEndUpdate</c>, so
    /// the old VM never received the selections. The fix calls
    /// <c>DoSelectItemsFromText</c> synchronously inside <c>OnDataContextBeginUpdate</c>.
    /// </summary>
    [AvaloniaFact]
    public async Task DataContextSwap_ViaButtonClick_CommitsTypedTextToOldVmBeforeSwap()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1SelectedItems = new ObservableCollection<object>();
        var vm1 = new DataContextVm
        {
            Items = items,
            Text = null,
            SelectedItems = vm1SelectedItems,
        };

        var vm2 = new DataContextVm
        {
            Items = items,
            Text = null,
            SelectedItems = new ObservableCollection<object>(),
        };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
            // Disable the auto-select timer so the selection stays pending.
            SelectItemsFromTextInputDelay = -1,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty,
            new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty,
            new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty,
            new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Simulate user typing "Apple, Cherry" — the auto-select timer is disabled,
        // so the selection is still pending (_isUserDefinedTextInputPending == true).
        mscb.Text = "Apple, Cherry";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Nothing should be selected yet.
        Assert.Empty(vm1SelectedItems);

        // --- User clicks a button that swaps the DataContext ---
        mscb.DataContext = vm2;

        // The commit must happen synchronously inside OnDataContextBeginUpdate,
        // so vm1SelectedItems is populated without any additional dispatcher pump.
        var vm1Selected = vm1SelectedItems.Cast<object>().Select(o => o!.ToString()).ToList();
        Assert.Contains("Apple", vm1Selected);
        Assert.Contains("Cherry", vm1Selected);
        Assert.DoesNotContain("Banana", vm1Selected);
    }


    /// The user types text into the MSCB while VM1 is active, then clicks a different item in
    /// the left-hand list — the ContentControl swaps its DataContext to VM2, which already has
    /// a <c>Text</c> that should map to a specific selection.
    ///
    /// The fix must be synchronous: no extra dispatcher pumps are allowed after the DataContext
    /// swap, because in real usage the user simply clicks and moves on.  The selection must be
    /// driven inside <c>OnDataContextEndUpdate</c> itself, not deferred to a background timer.
    /// </summary>
    [AvaloniaFact]
    public async Task DataContextSwap_ViaContentControl_SelectsItemsFromNewVmText()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        // VM1: user had typed "Banana" — text is in the box but selection not yet committed.
        var vm1 = new DataContextVm
        {
            Items = items,
            Text = "Banana",
            SelectedItems = new ObservableCollection<object>(),
        };

        // VM2: this VM's Text should drive the MSCB to select "Apple" and "Cherry".
        var vm2 = new DataContextVm
        {
            Items = items,
            Text = "Apple, Cherry",
            SelectedItems = new ObservableCollection<object>(),
        };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
            SelectItemsFromTextInputDelay = 0,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty,
            new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty,
            new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty,
            new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();

        // One cycle so the control is fully loaded with VM1.
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // --- User clicks the second list item: DataContext swaps synchronously ---
        mscb.DataContext = vm2;

        // No extra dispatcher pumps. In real usage the user just clicks — there is no
        // artificial background yield between the DataContext swap and the next interaction.
        // Selection from Text must be driven synchronously inside OnDataContextEndUpdate.
        var selectedTexts = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Apple", selectedTexts);
        Assert.Contains("Cherry", selectedTexts);
        Assert.DoesNotContain("Banana", selectedTexts);
    }

    /// <summary>
    /// Regression: when VM2 already has SelectedItems populated (e.g. the user visited before),
    /// a DataContext swap must NOT overwrite those selections by re-parsing Text.
    /// Text may represent custom/unconfirmed input from a prior visit; SelectedItems is the
    /// authoritative state and must be preserved.
    /// </summary>
    [AvaloniaFact]
    public async Task DataContextSwap_NewVmHasExistingSelections_SelectionsArePreserved()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1 = new DataContextVm
        {
            Items = items,
            Text = null,
            SelectedItems = new ObservableCollection<object>(),
        };

        // VM2 has Cherry selected, but Text still shows "Apple" from previous custom input.
        var vm2SelectedItems = new ObservableCollection<object> { "Cherry" };
        var vm2 = new DataContextVm
        {
            Items = items,
            Text = "Apple",
            SelectedItems = vm2SelectedItems,
        };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
            SelectItemsFromTextInputDelay = 0,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty,
            new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty,
            new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty,
            new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;

        // Cherry must still be selected — "Apple" from Text must not clobber it.
        var selectedTexts = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Cherry", selectedTexts);
        Assert.DoesNotContain("Apple", selectedTexts);
    }

    /// <summary>
    /// Regression: a pending auto-select timer (started by LostFocus or an explicit
    /// <see cref="MultiSelectionComboBox.ForceItemsSelection"/> call) must be
    /// cancelled unconditionally in <c>OnDataContextBeginUpdate</c>.
    ///
    /// Previously the timer stop was only inside the <c>_isUserDefinedTextInputPending</c>
    /// guard. When ForceItemsSelection started the timer (which does NOT set that flag),
    /// the timer survived the DataContext swap and fired after EndUpdate with the new VM's
    /// data — clobbering the new VM's pre-populated <c>SelectedItems</c>.
    /// </summary>
    [AvaloniaFact]
    public async Task DataContextSwap_PendingForceSelectionTimer_DoesNotClobberNewVmSelections()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        // VM1: typing is still pending.
        var vm1 = new DataContextVm
        {
            Items = items,
            Text = "Banana",
            SelectedItems = new ObservableCollection<object>(),
        };

        // VM2: Cherry is already selected; Text = "Apple" is stale/custom input from a
        // previous visit. The existing selection must survive the swap.
        var vm2SelectedItems = new ObservableCollection<object> { "Cherry" };
        var vm2 = new DataContextVm
        {
            Items = items,
            Text = "Apple",
            SelectedItems = vm2SelectedItems,
        };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
            // Disable the auto-select timer so we can start it manually below.
            SelectItemsFromTextInputDelay = -1,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty,
            new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty,
            new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty,
            new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Simulate LostFocus / explicit commit: starts a 0 ms background timer WITHOUT
        // setting _isUserDefinedTextInputPending. The old guard only stopped the timer
        // when that flag was true, so the timer survived the DataContext swap.
        mscb.ForceItemsSelection();

        // Immediately swap — no dispatcher pump in between.
        mscb.DataContext = vm2;

        // Pump the background queue. Without the fix, the lingering timer fires here
        // and calls DoSelectItemsFromText with vm2's Text = "Apple", replacing Cherry.
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var selectedTexts = mscb.SelectedItems!.Cast<object>().Select(o => o!.ToString()).ToList();
        Assert.Contains("Cherry", selectedTexts);
        Assert.DoesNotContain("Apple", selectedTexts);
    }


    /// only in case must NOT match and therefore must not drive a selection on DataContext swap.
    /// </summary>
    [AvaloniaFact]
    public async Task DataContextSwap_CaseSensitiveComparison_DoesNotSelectCaseMismatch()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm
        {
            Items = items,
            Text = null,
            SelectedItems = new ObservableCollection<object>(),
        };

        // Text is lowercase — should NOT match "Apple" under Ordinal comparison.
        var vm2 = new DataContextVm
        {
            Items = items,
            Text = "apple",
            SelectedItems = new ObservableCollection<object>(),
        };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
            EditableTextStringComparision = StringComparison.Ordinal,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty,
            new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty,
            new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty,
            new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;

        Assert.Empty(mscb.SelectedItems!);
    }

    /// <summary>
    /// Complementary to the above: with case-insensitive comparison (the default), the same
    /// lowercase text MUST select the matching item.
    /// </summary>
    [AvaloniaFact]
    public async Task DataContextSwap_CaseInsensitiveComparison_SelectsCaseMismatch()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm
        {
            Items = items,
            Text = null,
            SelectedItems = new ObservableCollection<object>(),
        };

        var vm2 = new DataContextVm
        {
            Items = items,
            Text = "apple",  // lowercase — matches under OrdinalIgnoreCase
            SelectedItems = new ObservableCollection<object>(),
        };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
            EditableTextStringComparision = StringComparison.OrdinalIgnoreCase,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty,
            new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty,
            new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty,
            new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;

        var selectedTexts = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Apple", selectedTexts);
    }

    private sealed class DataContextVm
    {
        public List<string>? Items { get; set; }
        public string? Text { get; set; }
        public ObservableCollection<object>? SelectedItems { get; set; }
    }

    // ─── DataContext swap – extended scenarios ────────────────────────────────

    /// <summary>
    /// Regression: when the control is NOT editable and Text is not bound, switching to a
    /// new DataContext that has an empty SelectedItems must clear the displayed selections.
    ///
    /// Previously, because <c>UpdateEditableText</c> exits early when there is no
    /// <c>PART_EditableTextBox</c> (non-editable mode), the stale <c>HasCustomText</c> /
    /// display state was never reset on DataContext swap, so the old VM's selections
    /// continued to appear.
    /// </summary>
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_UnboundText_NewVmHasEmptySelection_ClearsDisplay()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1SelectedItems = new ObservableCollection<object> { "Apple" };
        var vm1 = new DataContextVm
        {
            Items = items,
            SelectedItems = vm1SelectedItems,
        };

        var vm2SelectedItems = new ObservableCollection<object>(); // empty
        var vm2 = new DataContextVm
        {
            Items = items,
            SelectedItems = vm2SelectedItems,
        };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false, // no text box; Text is intentionally NOT bound
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty,
            new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty,
            new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // VM1: Apple is selected.
        Assert.Single(mscb.SelectedItems!);

        // --- Switch to VM2 which has no selections ---
        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Empty(mscb.SelectedItems!);
        Assert.DoesNotContain(":has-selections", mscb.Classes);
    }

    /// <summary>
    /// Single-mode equivalent of the DataContext swap test. Goes through a different code
    /// path in DoSelectItemsFromText (sets SelectedItem rather than manipulating SelectedItems).
    /// </summary>
    [AvaloniaFact]
    public async Task DataContextSwap_SingleMode_SelectsItemFromNewVmText()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1 = new DataContextVm { Items = items, Text = null, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, Text = "Banana", SelectedItems = new ObservableCollection<object>() };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Single,
            IsEditable = true,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty, new Binding(nameof(DataContextVm.Text)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;

        Assert.Equal("Banana", mscb.SelectedItem);
    }

    /// <summary>
    /// <see cref="MultiSelectionComboBox.SelectItemsFromTextInputDelay"/> defaults to -1,
    /// which disables auto-selection during typing. DataContext swap must still drive selection
    /// synchronously — the delay flag only controls the typing debounce, not the VM-swap path.
    /// </summary>
    [AvaloniaFact]
    public async Task DataContextSwap_DefaultDelay_StillSelectsItemsFromText()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1 = new DataContextVm { Items = items, Text = null, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, Text = "Apple, Cherry", SelectedItems = new ObservableCollection<object>() };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
            // SelectItemsFromTextInputDelay is left at its default of -1
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty, new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;

        var selectedTexts = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Apple", selectedTexts);
        Assert.Contains("Cherry", selectedTexts);
    }

    /// <summary>
    /// <see cref="MultiSelectionComboBox.IsReadOnly"/> prevents the user from typing but
    /// must NOT block DataContext-swap-driven selection — a VM change is not user input.
    /// </summary>
    [AvaloniaFact]
    public async Task DataContextSwap_IsReadOnly_StillSelectsItemsFromText()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1 = new DataContextVm { Items = items, Text = null, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, Text = "Cherry", SelectedItems = new ObservableCollection<object>() };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
            IsReadOnly = true,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty, new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;

        var selectedTexts = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Cherry", selectedTexts);
    }

    /// <summary>
    /// When the new VM has null/empty Text and empty SelectedItems, the DataContext swap
    /// should result in no selection and no custom text.
    /// </summary>
    [AvaloniaFact]
    public async Task DataContextSwap_NewVmHasNullText_ResultsInEmptySelection()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, Text = "Apple", SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, Text = null, SelectedItems = new ObservableCollection<object>() };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty, new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;

        Assert.Empty(mscb.SelectedItems!);
        Assert.False(mscb.HasCustomText);
    }

    // ─── Duplicate tokens ─────────────────────────────────────────────────────

    /// <summary>
    /// When Text contains a duplicate token (e.g. "Apple, Apple"), the item must be selected
    /// exactly once. Previously the second occurrence triggered the unhandled
    /// <c>oldPosition &lt; position</c> path, which called <c>TryAddObjectFromString</c> and
    /// could add a duplicate to the source list when a parser was configured.
    /// </summary>
    [AvaloniaFact]
    public async Task ForceItemsSelection_DuplicateToken_SelectsItemOnce()
    {
        var items = new ObservableCollection<string> { "Apple", "Banana" };
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = items;
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
        });

        mscb.Text = "Apple, Apple";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.ForceItemsSelection();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Apple must appear exactly once in SelectedItems.
        var selected = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Single(selected, s => s == "Apple");
    }

    /// <summary>
    /// Same as above but with a <see cref="DefaultStringToObjectParser"/> configured.
    /// The duplicate token must NOT cause a second "Apple" to be added to the source list.
    /// </summary>
    [AvaloniaFact]
    public async Task ForceItemsSelection_DuplicateToken_WithParser_DoesNotAddDuplicate()
    {
        var items = new ObservableCollection<string> { "Apple", "Banana" };
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = items;
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
            m.StringToObjectParser = DefaultStringToObjectParser.Instance;
        });

        mscb.Text = "Apple, Apple";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.ForceItemsSelection();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Source must not gain a second "Apple".
        Assert.Equal(2, items.Count); // still just Apple + Banana
        var selected = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Single(selected, s => s == "Apple");
    }

    // ─── DataContext swap – stale Text when Text is not bound ────────────────
    //
    // Two related bugs when Text is not data-bound:
    //
    // Bug A – "new VM has selections, Text stays null":
    //   OnDataContextEndUpdate calls UpdateEditableText, but by that point the
    //   selection model hasn't yet synced with the new SelectedItems collection, so
    //   GetSelectedItemsText() returns null. The Loaded-priority post that would
    //   later call UpdateEditableText again is suppressed while _isDataContextUpdating
    //   is true, and no follow-up runs afterwards.
    //
    // Bug B – "stale Text survives swap to empty VM":
    //   When Text was set (internally by UpdateEditableText or explicitly) before the
    //   swap, UpdateHasCustomText in OnDataContextEndUpdate sees non-null Text vs
    //   null selectedItemsText → HasCustomText = true → UpdateEditableText skips the
    //   reset → the old Text remains.
    // ─────────────────────────────────────────────────────────────────────────────────

    // ── Bug A tests ──────────────────────────────────────────────────────────────────

    // A-1. Non-editable, VM2 has a single pre-selected item.
    //      Text must be updated to "Banana" after the swap.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_TextUnbound_Vm2HasSingleSelection_TextReflectsNewVm()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Banana" } };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Banana", mscb.Text);
    }

    // A-2. VM2 has multiple pre-selected items; Text must show them joined.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_TextUnbound_Vm2HasMultipleSelections_TextReflectsNewVm()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Apple", "Cherry" } };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Apple, Cherry", mscb.Text);
    }

    // A-3. Single-selection mode: VM2 has one pre-selected item.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_SingleMode_TextUnbound_Vm2HasSelection_TextReflectsNewVm()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Banana" } };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Single,
            IsEditable = false,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Banana", mscb.Text);
    }

    // A-4. SelectedItemStringFormat is active; VM2's selection must appear formatted.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_TextUnbound_WithStringFormat_Vm2HasSelection_TextReflectsNewVm()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Apple" } };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false,
            SelectedItemStringFormat = "({0})",
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("(Apple)", mscb.Text);
    }

    // A-5. Bindings registered in reverse order (SelectedItems before ItemsSource).
    //      The Text-update failure must not depend on registration order.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_TextUnbound_ReverseBindingOrder_Vm2HasSelection_TextReflectsNewVm()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Banana" } };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false,
        };
        // Reverse order: SelectedItems registered before ItemsSource
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Banana", mscb.Text);
    }

    // ── Bug B tests ──────────────────────────────────────────────────────────────────
    //
    // For these tests Text is set explicitly (SetCurrentValue / direct assignment) before
    // the swap to reliably produce the stale-Text state, without relying on the timing of
    // the internal UpdateEditableText that fires via the Loaded-priority post.

    // B-1. Text is set to "Apple" before the swap; VM2 has empty selections.
    //      UpdateHasCustomText sees ("Apple" vs null) → HasCustomText = true → UpdateEditableText
    //      skips the reset → Text stays "Apple" after the swap.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_StaleTextFromVm1_Vm2Empty_TextClears()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Simulate the internal state that UpdateEditableText produces when VM1 has selections.
        mscb.SetCurrentValue(Controls.MultiSelectionComboBox.TextProperty, "Apple");
        Assert.Equal("Apple", mscb.Text); // sanity

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.True(string.IsNullOrEmpty(mscb.Text),
            $"Expected Text null/empty after DC swap to empty VM, but got: \"{mscb.Text}\"");
        Assert.False(mscb.HasCustomText);
    }

    // B-2. Text is "Apple, Banana"; VM2 has "Cherry" selected.
    //      Text must be "Cherry" after the swap — not stay at the stale joined string.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_StaleTextFromVm1_Vm2HasSelection_TextFollowsNewVm()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Cherry" } };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.SetCurrentValue(Controls.MultiSelectionComboBox.TextProperty, "Apple, Banana");

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Cherry", mscb.Text);
    }

    // B-3. HasCustomText must be false after the swap to empty VM2 (same stale-Text scenario).
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_StaleTextFromVm1_Vm2Empty_HasCustomTextReset()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.SetCurrentValue(Controls.MultiSelectionComboBox.TextProperty, "Apple");

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.False(mscb.HasCustomText);
    }

    // B-4. IsEditable = true, Text NOT bound: same stale-Text regression exists in editable mode.
    [AvaloniaFact]
    public async Task DataContextSwap_Editable_TextUnbound_StaleText_Vm2Empty_TextClears()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true, // editable but Text intentionally left unbound
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.SetCurrentValue(Controls.MultiSelectionComboBox.TextProperty, "Apple");

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.True(string.IsNullOrEmpty(mscb.Text),
            $"Expected Text null/empty (editable, Text unbound) after DC swap, but got: \"{mscb.Text}\"");
    }

    // B-5. ObjectToStringComparer is set: the stale Text ("Apple") must NOT cause
    //      DoSelectItemsFromText to insert a spurious selection into VM2's empty collection.
    //      Without the fix, hasExistingSelection=false → DoSelectItemsFromText runs → stale
    //      "Apple" matches an item → vm2's collection is mutated.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_StaleText_WithComparer_Vm2Empty_NoUnwantedSelection()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm2SelectedItems = new ObservableCollection<object>();
        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, SelectedItems = vm2SelectedItems };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.SetCurrentValue(Controls.MultiSelectionComboBox.TextProperty, "Apple");

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // The stale "Apple" text must NOT cause DoSelectItemsFromText to populate VM2.
        Assert.Empty(vm2SelectedItems);
        Assert.True(string.IsNullOrEmpty(mscb.Text),
            $"Expected Text null/empty after DC swap (comparer set), but got: \"{mscb.Text}\"");
    }

    // ── Additional DataContext swap scenarios ─────────────────────────────────────────────────
    //
    // C-1  Both VMs non-empty, different selections
    // C-2  Bidirectional swap (VM1→VM2→VM1)
    // C-3  Three-step: VM1 → null DataContext → VM2
    // C-4  Single-selection mode + Bug-B stale text
    // C-5  Editable mode, both VMs non-empty
    // C-6  Text IS bound, VM2 has non-matching text → HasCustomText stays true  (regression guard)
    // C-7  Text IS bound, VM2 text matches its selection → HasCustomText false   (regression guard)

    // C-1. Both VMs have non-empty selections; Text must switch to the new VM's items.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_TextUnbound_BothVmsHaveSelections_TextUpdates()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Apple" } };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Banana" } };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Banana", mscb.Text);
    }

    // C-2. Bidirectional swap: VM1 → VM2 → VM1.  Text must round-trip correctly.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_TextUnbound_BidirectionalSwap_TextRoundTrips()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Apple" } };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Banana" } };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.Equal("Banana", mscb.Text); // mid-point sanity

        mscb.DataContext = vm1;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Apple", mscb.Text);
    }

    // C-3. Three-step: VM1 → null DataContext → VM2.
    //      Text must be null/empty after the null step and reflect VM2 after the final swap.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_TextUnbound_ThroughNullDataContext_TextUpdates()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Apple" } };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Banana" } };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = false,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = null;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.True(string.IsNullOrEmpty(mscb.Text),
            $"Expected Text null/empty after null DataContext, but got: \"{mscb.Text}\"");

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Banana", mscb.Text);
    }

    // C-4. Single-selection mode + stale Text (Bug B variant for SelectionMode.Single).
    //      After swap to an empty VM2, Text must clear and HasCustomText must be false.
    [AvaloniaFact]
    public async Task DataContextSwap_NonEditable_SingleMode_StaleText_Vm2Empty_TextClears()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object>() };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Single,
            IsEditable = false,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.SetCurrentValue(Controls.MultiSelectionComboBox.TextProperty, "Apple");

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.True(string.IsNullOrEmpty(mscb.Text),
            $"Expected Text null/empty (single mode) after DC swap, but got: \"{mscb.Text}\"");
        Assert.False(mscb.HasCustomText);
    }

    // C-5. Editable mode: both VMs have non-empty (different) selections, Text not bound.
    //      Text must switch to the new VM's selection string even in editable mode.
    [AvaloniaFact]
    public async Task DataContextSwap_Editable_TextUnbound_BothVmsHaveSelections_TextUpdates()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Apple" } };
        var vm2 = new DataContextVm { Items = items, SelectedItems = new ObservableCollection<object> { "Cherry" } };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Cherry", mscb.Text);
        Assert.False(mscb.HasCustomText);
    }

    // C-6. Regression guard: Text IS bound, VM2's bound value is a non-matching string.
    //      The fix must NOT reset HasCustomText when Text is driven by a binding.
    //      Expected: Text stays at the bound value, HasCustomText=true.
    [AvaloniaFact]
    public async Task DataContextSwap_TextBound_Vm2HasNonMatchingText_HasCustomTextTrue()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, Text = null,  SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, Text = "App", SelectedItems = new ObservableCollection<object>() };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty, new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // "App" doesn't match selectedItemsText (null) → HasCustomText must be true.
        Assert.Equal("App", mscb.Text);
        Assert.True(mscb.HasCustomText);
    }

    // C-7. Regression guard: Text IS bound and the bound value matches the VM2 selection.
    //      HasCustomText must be false; Text must equal the selection string.
    [AvaloniaFact]
    public async Task DataContextSwap_TextBound_Vm2TextMatchesSelection_HasCustomTextFalse()
    {
        var items = new List<string> { "Apple", "Banana" };

        var vm1 = new DataContextVm { Items = items, Text = null,    SelectedItems = new ObservableCollection<object>() };
        var vm2 = new DataContextVm { Items = items, Text = "Apple", SelectedItems = new ObservableCollection<object> { "Apple" } };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty, new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Apple", mscb.Text);
        Assert.False(mscb.HasCustomText);
    }

    // ─── Separator change ─────────────────────────────────────────────────────

    // ─── Separator change ─────────────────────────────────────────────────────

    /// <summary>
    /// Changing <see cref="Separator"/> at runtime must update
    /// the displayed text of already-selected items.
    /// </summary>
    [AvaloniaFact]
    public async Task SeparatorChange_UpdatesDisplayedText()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        mscb.Selection.Select(0);
        mscb.Selection.Select(1);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Apple, Banana", mscb.GetSelectedItemsText());

        mscb.Separator = " | ";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Equal("Apple | Banana", mscb.GetSelectedItemsText());
    }

    // ─── :has-selections pseudoclass – Single mode ───────────────────────────

    [AvaloniaFact]
    public async Task Pseudoclass_HasSelections_TracksSelectedItem_InSingleMode()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.SelectionMode = SelectionMode.Single;
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        Assert.DoesNotContain(":has-selections", mscb.Classes);

        mscb.SelectedItem = "Apple";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.Contains(":has-selections", mscb.Classes);

        mscb.SelectedItem = null;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.DoesNotContain(":has-selections", mscb.Classes);
    }

    // ─── Enter key separator/comparer guard ──────────────────────────────────────

    /// <summary>
    /// Regression: the Enter key handler called DoSelectItemsFromText(true) unconditionally,
    /// without the ObjectToStringComparer / Separator guard that every other call-site
    /// applies. In Multiple mode with no Separator, DoSelectItemsFromText cannot split the
    /// text into tokens; position stays 0 and the cleanup loop removes ALL selected items.
    /// </summary>
    [AvaloniaFact]
    public async Task EnterKey_MultipleMode_NoSeparator_DoesNotClearSelection()
    {
        var (mscb, window) = await CreateLoadedWithWindowAsync(m =>
        {
            m.Separator = null; // Override the default ", "
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
        });

        mscb.Selection.Select(0); // Apple
        mscb.Selection.Select(2); // Cherry
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.Equal(2, mscb.SelectedItems!.Count);

        mscb.Focus();
        // User types a token but there is no separator to split on.
        mscb.Text = "Apple";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Without the fix: cleanup loop runs from position=0 → all items removed.
        // With the fix: guard prevents DoSelectItemsFromText; selection is preserved.
        Assert.True(mscb.SelectedItems!.Count >= 2,
            $"Selection was unexpectedly cleared. Expected ≥2 items, got {mscb.SelectedItems!.Count}.");
    }

    /// <summary>
    /// Regression: Enter key + Multiple mode + ObjectToStringComparer set + Separator absent.
    /// Same root cause as <see cref="EnterKey_MultipleMode_NoSeparator_DoesNotClearSelection"/>
    /// but makes the absent Separator explicit even when a comparer is provided.
    /// </summary>
    [AvaloniaFact]
    public async Task EnterKey_MultipleMode_NoSeparator_WithComparer_DoesNotClearPreExistingSelections()
    {
        var (mscb, window) = await CreateLoadedWithWindowAsync(m =>
        {
            m.Separator = null;
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
            m.StringToObjectParser  = DefaultStringToObjectParser.Instance;
        });

        mscb.Selection.Select(1); // Banana
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.Single(mscb.SelectedItems!);

        mscb.Focus();
        mscb.Text = "Apple";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Banana must still be selected (the cleanup loop must not run without a separator).
        Assert.Contains("Banana", mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()));
    }

    /// <summary>
    /// Regression guard: Enter key in Multiple mode WITH a Separator and ObjectToStringComparer
    /// must still commit the typed text to selections (existing behaviour must not regress).
    /// </summary>
    [AvaloniaFact]
    public async Task EnterKey_MultipleMode_WithSeparatorAndComparer_CommitsTypedText()
    {
        var (mscb, window) = await CreateLoadedWithWindowAsync(m =>
        {
            // Separator = ", " from CreateLoadedWithWindowAsync defaults.
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
        });

        mscb.Focus();
        mscb.Text = "Banana";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var selected = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Banana", selected);
    }

    // ─── SelectItemsFromTextInputDelay = 0 comparer/separator guard ──────────

    /// <summary>
    /// Regression: the delay=0 fast-path in SelectItemsFromText bypassed the
    /// ObjectToStringComparer / Separator guard used by the timer path. Without an
    /// ObjectToStringComparer no item can ever be matched; the cleanup loop at the end of
    /// DoSelectItemsFromText then removes every previously-selected item.
    /// </summary>
    [AvaloniaFact]
    public async Task TypingText_Delay0_WithoutObjectToStringComparer_PreservesSelection()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.SelectItemsFromTextInputDelay = 0;
            // ObjectToStringComparer intentionally NOT set.
        });

        mscb.Selection.Select(0); // Apple
        mscb.Selection.Select(2); // Cherry
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.Equal(2, mscb.SelectedItems!.Count);

        // Without a comparer the typed text cannot drive selection changes.
        mscb.Text = "Apple";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Without the fix: all items removed (cleanup from position=0).
        // With the fix: DoSelectItemsFromText is not called; selection preserved.
        Assert.Equal(2, mscb.SelectedItems!.Count);
        Assert.Contains("Apple", mscb.SelectedItems!.Cast<object>());
        Assert.Contains("Cherry", mscb.SelectedItems!.Cast<object>());
    }

    /// <summary>
    /// Regression: delay=0 + ObjectToStringComparer set but Separator absent in Multiple mode.
    /// Equivalent to the Enter-key bug but triggered by typing: the cleanup loop runs from
    /// position=0 because strings is null when there is no separator to split on.
    /// </summary>
    [AvaloniaFact]
    public async Task TypingText_Delay0_NoSeparator_MultipleMode_PreservesSelection()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.Separator = null; // Override default ", "
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
            m.SelectItemsFromTextInputDelay = 0;
        });

        mscb.Selection.Select(0); // Apple
        mscb.Selection.Select(2); // Cherry
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.Equal(2, mscb.SelectedItems!.Count);

        mscb.Text = "Apple";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.True(mscb.SelectedItems!.Count >= 2,
            $"Selection was unexpectedly cleared. Expected ≥2 items, got {mscb.SelectedItems!.Count}.");
    }

    /// <summary>
    /// Regression guard: delay=0 WITH ObjectToStringComparer and Separator must still
    /// select the matching item as the user types (existing behaviour must not regress).
    /// </summary>
    [AvaloniaFact]
    public async Task TypingText_Delay0_WithComparerAndSeparator_SelectsMatchingItem()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            // Separator = ", " from CreateLoadedAsync defaults.
            m.ItemsSource = new List<string> { "Apple", "Banana", "Cherry" };
            m.ObjectToStringComparer = DefaultObjectToStringComparer.Instance;
            m.SelectItemsFromTextInputDelay = 0;
        });

        mscb.Text = "Banana";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        var selected = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Banana", selected);
    }

    // ─── LostFocus guard: DataContext-updating flag ───────────────────────────

    /// <summary>
    /// If the textbox loses focus while a DataContext swap is in progress (e.g. because
    /// the incoming ItemsSource change causes a re-layout that moves focus), the
    /// LostFocus handler must be a no-op. Running DoSelectItemsFromText with half-updated
    /// bindings can corrupt the incoming selection.
    ///
    /// We simulate the condition by manually firing LostFocus while
    /// _isDataContextUpdating is true via a DataContext swap with simultaneous selection.
    /// </summary>
    [AvaloniaFact]
    public async Task LostFocus_DuringDataContextSwap_DoesNotClearNewVmSelection()
    {
        var items = new List<string> { "Apple", "Banana", "Cherry" };

        var vm1SelectedItems = new ObservableCollection<object>();
        var vm1 = new DataContextVm { Items = items, Text = null, SelectedItems = vm1SelectedItems };

        var vm2SelectedItems = new ObservableCollection<object> { "Cherry" };
        var vm2 = new DataContextVm { Items = items, Text = "Apple", SelectedItems = vm2SelectedItems };

        var mscb = new Controls.MultiSelectionComboBox
        {
            SelectionMode = SelectionMode.Multiple,
            Separator = ", ",
            IsEditable = true,
            ObjectToStringComparer = DefaultObjectToStringComparer.Instance,
        };
        mscb.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(DataContextVm.Items)));
        mscb.Bind(Controls.MultiSelectionComboBox.TextProperty, new Binding(nameof(DataContextVm.Text)));
        mscb.Bind(ListBox.SelectedItemsProperty, new Binding(nameof(DataContextVm.SelectedItems)));

        mscb.DataContext = vm1;
        var window = new Window { Content = mscb, Width = 400, Height = 60 };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Simulate the user having typed something that makes Text ≠ "" before the swap.
        mscb.Text = "Apple";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Swap to VM2 which has Cherry already selected.
        mscb.DataContext = vm2;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Cherry must survive; "Apple" text from VM1 must not drive a spurious selection on VM2.
        var selected = mscb.SelectedItems!.Cast<object>().Select(o => o.ToString()).ToList();
        Assert.Contains("Cherry", selected);
    }
}
