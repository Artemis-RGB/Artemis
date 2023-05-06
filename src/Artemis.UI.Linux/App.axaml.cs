using Artemis.Core.Services;
using Artemis.UI.Linux.DryIoc;
using Artemis.UI.Linux.Providers.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DryIoc;
using ReactiveUI;

namespace Artemis.UI.Linux;

public class App : Application
{
    private ApplicationStateManager? _applicationStateManager;
    private IContainer? _container;

    public override void Initialize()
    {
        _container = ArtemisBootstrapper.Bootstrap(this, c => c.RegisterProviders());
        Program.CreateLogger(_container);
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
            return;
        
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) 
            return;
        
        ArtemisBootstrapper.Initialize();

        _applicationStateManager = new ApplicationStateManager(_container!, desktop.Args);
        RegisterProviders();
    }

    private void RegisterProviders()
    {
        IInputService inputService = _container.Resolve<IInputService>();
        try
        {
            inputService.AddInputProvider(_container.Resolve<InputProvider>(LinuxInputProvider.Id));
        }
        catch
        {
            //TODO: handle not having permissions for the input file.
        }
    }
}