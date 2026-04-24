using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Xunit;

namespace Avalonia.Labs.Controls.Tests.MultiSelectionComboBox;

public class MultiSelectionComboBoxTests
{
    private static async Task<Controls.MultiSelectionComboBox> CreateLoadedAsync(
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

        return mscb;
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

        Assert.False(mscb.Classes.Contains(":has-custom-text"));

        mscb.Text = "different text";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.True(mscb.Classes.Contains(":has-custom-text"));

        mscb.Text = "";
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.False(mscb.Classes.Contains(":has-custom-text"));
    }

    [AvaloniaFact]
    public async Task Pseudoclass_Multiple_TracksSelectionMode()
    {
        var mscb = await CreateLoadedAsync();

        Assert.True(mscb.Classes.Contains(":multiple"));

        mscb.SelectionMode = SelectionMode.Single;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.False(mscb.Classes.Contains(":multiple"));

        mscb.SelectionMode = SelectionMode.Multiple;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.True(mscb.Classes.Contains(":multiple"));
    }

    [AvaloniaFact]
    public async Task Pseudoclass_HasSelections_TracksSelectedItems()
    {
        var mscb = await CreateLoadedAsync(m =>
        {
            m.ItemsSource = new List<string> { "Apple", "Banana" };
        });

        Assert.False(mscb.Classes.Contains(":has-selections"));

        mscb.Selection.Select(0);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.True(mscb.Classes.Contains(":has-selections"));

        mscb.Selection.Clear();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.False(mscb.Classes.Contains(":has-selections"));
    }

    [AvaloniaFact]
    public async Task Pseudoclass_Editable_TracksIsEditable()
    {
        var mscb = await CreateLoadedAsync();

        Assert.True(mscb.Classes.Contains(":editable"));

        mscb.IsEditable = false;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.False(mscb.Classes.Contains(":editable"));

        mscb.IsEditable = true;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Assert.True(mscb.Classes.Contains(":editable"));
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

        Assert.Equal(1, mscb.SelectedItems!.Count);
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
        Assert.Equal(1, mscb.SelectedItems!.Count);
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
        Assert.Equal(1, mscb.SelectedItems!.Count);
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

        Assert.Equal(0, mscb.SelectedItems!.Count);
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
        Assert.Equal(0, mscb.SelectedItems!.Count);
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
}
