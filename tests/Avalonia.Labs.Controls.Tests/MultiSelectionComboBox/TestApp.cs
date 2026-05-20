using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Labs.Controls;
using Avalonia.Themes.Fluent;

[assembly: AvaloniaTestFramework]

namespace Avalonia.Labs.Controls.Tests.MultiSelectionComboBox;

public class TestApp
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
}

public class TestApplication : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new ControlThemes());
    }
}
