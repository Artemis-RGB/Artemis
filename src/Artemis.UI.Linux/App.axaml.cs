using Artemis.Core.Services;
using Artemis.UI.Linux.Providers.Input;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;
using Ninject;
using Avalonia.Controls.ApplicationLifetimes;

namespace Artemis.UI.Linux
{
    public class App : Application
    {
        private StandardKernel? _kernel;
        private ApplicationStateManager? _applicationStateManager;

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
            ArtemisBootstrapper.Initialize();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                _applicationStateManager = new ApplicationStateManager(_kernel!, desktop.Args);
        }
        
        private void RegisterProviders()
        {
            IInputService inputService = _kernel.Get<IInputService>();
            // inputService.AddInputProvider(_kernel.Get<LinuxInputProvider>());
        }
    }
}