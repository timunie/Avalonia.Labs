using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Labs.Controls;
using Avalonia.Labs.Controls.Base;
using Avalonia.VisualTree;

namespace Avalonia.Labs.Catalog.Views
{
    public partial class SwipeView : UserControl
    {
        public SwipeView()
        {
            InitializeComponent();
        }

        private void TapGestureRecognizer_OnOnTap(object? sender, TapEventArgs e)
        {
            if (sender is StackPanel panel)
            {
                var label = panel.Children.OfType<Label>().First();
                label.Content = "Clicked";
            }
        }

        private void CloseSwipe(object? sender, RoutedEventArgs e)
        {
            var demoSwipe = (sender as Visual).FindAncestorOfType<Swipe>() ??
                this.FindControl<Swipe>("DemoSwipe");
            if (demoSwipe != null)
            {
                demoSwipe.SwipeState = SwipeState.Hidden;
            }
        }

        private void OpenLeft(object? sender, RoutedEventArgs e)
        {
            var demoSwipe = this.FindControl<Swipe>("DemoSwipe");
            if (demoSwipe != null)
            {
                demoSwipe.SwipeState = SwipeState.LeftVisible;
            }
        }

        private void OpenRight(object? sender, RoutedEventArgs e)
        {
            var demoSwipe = this.FindControl<Swipe>("DemoSwipe");
            if (demoSwipe != null)
            {
                demoSwipe.SwipeState = SwipeState.RightVisible;
            }
        }

        private void OpenTop(object? sender, RoutedEventArgs e)
        {
            var demoSwipe = this.FindControl<Swipe>("DemoSwipe");
            if (demoSwipe != null)
            {
                demoSwipe.SwipeState = SwipeState.TopVisible;
            }
        }

        private void OpenBottom(object? sender, RoutedEventArgs e)
        {
            var demoSwipe = this.FindControl<Swipe>("DemoSwipe");
            if (demoSwipe != null)
            {
                demoSwipe.SwipeState = SwipeState.BottomVisible;
            }
        }

        private void DisableEnableSwipe(object? sender, RoutedEventArgs e)
        {
            if (this.FindControl<Swipe>("DemoSwipe") is { } demoswipe)
            {
                demoswipe.IsSwipeEnabled = !demoswipe.IsSwipeEnabled;
                if (sender is Button button)
                {
                    button.Content = demoswipe.IsSwipeEnabled switch
                    {
                        true => "Disable",
                        false => "Enable"
                    };
                }
            }
        }

        private void DemoSwipe_OpenRequested(object? sender, OpenRequestedEventArgs e)
        {
            if(DataContext is ViewModels.SwipeViewModel vm)
            {
                vm.LastEvent = $"OpenRequested: {e.OpenSwipeItem}";
            } 
        }

        private void DemoSwipe_CloseRequested(object? sender, CloseRequestedEventArgs e)
        {
            if (DataContext is ViewModels.SwipeViewModel vm)
            {
                vm.LastEvent = "CloseRequested";
            }
        }
    }
}
