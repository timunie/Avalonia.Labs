using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Labs.Catalog.Views;
using Avalonia.Labs.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalonia.Labs.Catalog.ViewModels;

public partial class MultiSelectionComboBoxViewModel : ViewModelBase
{
    static MultiSelectionComboBoxViewModel()
    {
        ViewLocator.Register(typeof(MultiSelectionComboBoxViewModel), () => new MultiSelectionComboBoxView());
    }

    public MultiSelectionComboBoxViewModel()
    {
        Title = "MultiSelectionComboBox";

        SelectedFrameworks.CollectionChanged += (s, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems)
                {
                    System.Diagnostics.Debug.WriteLine($"Added: {item}");
                }
            }

            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems)
                {
                    System.Diagnostics.Debug.WriteLine($"Removed: {item}");
                }
            }
        };
    }

    public ObservableCollection<string> Frameworks { get; } =
    [
        "Avalonia UI", "Blazor", "Flutter", "Ionic",
        "Jetpack Compose", "MAUI", ".NET MAUI", "React Native",
        "SwiftUI", "Uno Platform", "WPF", "WinUI 3",
        "Xamarin", "Angular", "Vue.js", "Svelte",
    ];

    public ObservableCollection<string> SelectedFrameworks { get; } = new();

    public IReadOnlyList<SelectedItemsOrderType> OrderTypes { get; } = System.Enum.GetValues<SelectedItemsOrderType>();

    [ObservableProperty] public partial bool IsEditable { get; set; } = false;

    [ObservableProperty] public partial bool IsReadOnly { get; set; } = false;

    [ObservableProperty] public partial bool ShowClearButton { get; set; } = true;

    [ObservableProperty]
    public partial SelectedItemsOrderType OrderSelectedItemsBy { get; set; } = SelectedItemsOrderType.SelectedOrder;

    [ObservableProperty] public partial int SelectItemsFromTextInputDelay { get; set; } = 200;
}
