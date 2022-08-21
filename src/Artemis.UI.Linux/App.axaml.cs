using Artemis.Core.Services;
using Artemis.UI.Linux.Providers.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Linux;

public class App : Application
{
    private ApplicationStateManager? _applicationStateManager;
    private StandardKernel? _kernel;

    public override void Initialize()
    {
        _kernel = ArtemisBootstrapper.Bootstrap(this);
        Program.CreateLogger(_kernel);
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
            _applicationStateManager = new ApplicationStateManager(_kernel!, desktop.Args);
    }

    private void RegisterProviders()
    {
        IInputService inputService = _kernel.Get<IInputService>();
        try
        {
            inputService.AddInputProvider(_kernel.Get<LinuxInputProvider>());
        }
        catch
        {
            //TODO: handle not having permissions for the input file.
        }
    }
}