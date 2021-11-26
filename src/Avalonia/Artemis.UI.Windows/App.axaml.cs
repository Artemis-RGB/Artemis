using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Windows
{
    public class App : Application
    {
        // ReSharper disable NotAccessedField.Local
        private StandardKernel? _kernel;
        private ApplicationStateManager? _applicationStateManager;
        // ReSharper restore NotAccessedField.Local

        public override void Initialize()
        {
            _kernel = ArtemisBootstrapper.Bootstrap(this);
            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ArtemisBootstrapper.Initialized();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                _applicationStateManager = new ApplicationStateManager(_kernel!, desktop.Args);
        }
    }
}