using Artemis.Core.Services;
using Artemis.UI.Linux.Providers.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DryIoc;
using ReactiveUI;

namespace Artemis.UI.Linux;

public class App : Application
{
    private ApplicationStateManager? _applicationStateManager;
    private IContainer? _container;

    public override void Initialize()
    {
        _container = ArtemisBootstrapper.Bootstrap(this);
        Program.CreateLogger(_container);
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        AvaloniaXamlLoader.Load(this);

        RegisterProviders();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
            return;

        ArtemisBootstrapper.Initialize();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            _applicationStateManager = new ApplicationStateManager(_container!, desktop.Args);
    }

    private void RegisterProviders()
    {
        IInputService inputService = _container.Resolve<IInputService>();
        try
        {
            inputService.AddInputProvider(_container.Resolve<LinuxInputProvider>());
        }
        catch
        {
            //TODO: handle not having permissions for the input file.
        }
    }
}