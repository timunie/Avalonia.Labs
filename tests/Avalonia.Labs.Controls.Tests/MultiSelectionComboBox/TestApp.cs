using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Labs.Controls;
using Avalonia.Themes.Fluent;

[assembly: AvaloniaTestFramework(typeof(Avalonia.Labs.Controls.Tests.MultiSelectionComboBox.TestApp))]

namespace Avalonia.Labs.Controls.Tests.MultiSelectionComboBox;

public class TestApp
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessOptions { UseHeadlessDrawing = true });
}

public class TestApplication : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new ControlThemes());
    }
}
