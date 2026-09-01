using System.ComponentModel;
using System.Linq;
using Avalonia.Labs.Catalog.Views;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalonia.Labs.Catalog.ViewModels;

public partial class SwipeViewModel : ViewModelBase
{
    static SwipeViewModel()
    {
        ViewLocator.Register(typeof(SwipeViewModel), () => new SwipeView());
    }

    public SwipeViewModel()
    {
        Title = "Swipe";
        Items = Enumerable.Range(0, 20).ToArray();
    }

    public int[] Items
    {
        get;
        set;
    }

    [ObservableProperty]
    public partial Labs.Controls.SwipeMode SwipeMode { get; private set; } = Labs.Controls.SwipeMode.Reveal;

    [ObservableProperty]
    public partial bool IsExecute { get; set; } = false;

    [ObservableProperty]
    public partial string? LastEvent { get; set; }

    public void SwipeCommand(object? parameter) =>
        LastEvent = $"{parameter}Command is executed";

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if(e.PropertyName == nameof(IsExecute))
        {
            SwipeMode = IsExecute ? Labs.Controls.SwipeMode.Execute : Labs.Controls.SwipeMode.Reveal;
        }
    }
     
}
