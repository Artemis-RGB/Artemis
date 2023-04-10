using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DryIoc;
using ReactiveUI;

namespace Artemis.UI.MacOS;

public class App : Application
{
    private IContainer? _container;

    public override void Initialize()
    {
        _container = ArtemisBootstrapper.Bootstrap(this);
        Program.CreateLogger(_container);
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