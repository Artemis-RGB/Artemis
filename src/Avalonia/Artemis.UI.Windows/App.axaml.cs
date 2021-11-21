using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FluentAvalonia.Styling;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Windows
{
    public class App : Application
    {
        private StandardKernel _kernel;
        private ApplicationStateManager _stateManager;

        public override void Initialize()
        {
            _kernel = ArtemisBootstrapper.Bootstrap();
            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                ArtemisBootstrapper.ConfigureApplicationLifetime(desktop);
                AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().ForceNativeTitleBarToTheme(desktop.MainWindow, "Dark");

                _stateManager = new ApplicationStateManager(_kernel, desktop.Args);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}