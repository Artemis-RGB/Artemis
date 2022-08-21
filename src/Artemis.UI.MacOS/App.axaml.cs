using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.MacOS;

public class App : Application
{
    private StandardKernel? _kernel;

    public override void Initialize()
    {
        _kernel = ArtemisBootstrapper.Bootstrap(this);
        Program.CreateLogger(_kernel);
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
            return;

        ArtemisBootstrapper.Initialize();
    }
}