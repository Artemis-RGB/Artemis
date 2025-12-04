using System;
using Artemis.Core;
using Artemis.Storage;
using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using DryIoc;
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
            StorageManager.CreateBackup(Constants.DataFolder);
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
        return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().UseReactiveUI();
    }

    public static void CreateLogger(IContainer container)
    {
        Logger = container.Resolve<ILogger>().ForContext<Program>();
    }
}