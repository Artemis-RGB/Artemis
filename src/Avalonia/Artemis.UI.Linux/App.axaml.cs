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
            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ArtemisBootstrapper.Initialize();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                _applicationStateManager = new ApplicationStateManager(_kernel!, desktop.Args);
        }
    }
}