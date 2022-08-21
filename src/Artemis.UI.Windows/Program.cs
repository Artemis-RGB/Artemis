using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Ninject;
using Serilog;

namespace Artemis.UI.Windows;

internal class Program
{
    private static ILogger? Logger { get; set; }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Logger?.Fatal(e, "Fatal exception, shutting down");
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .With(new Win32PlatformOptions
            {
                UseWindowsUIComposition = true,
                CompositionBackdropCornerRadius = 8f
            })
            .UseReactiveUI();
    }

    public static void CreateLogger(IKernel kernel)
    {
        Logger = kernel.Get<ILogger>().ForContext<Program>();
    }
}