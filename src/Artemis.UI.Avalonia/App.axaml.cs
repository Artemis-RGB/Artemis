using Artemis.Core.Ninject;
using Artemis.UI.Avalonia.Ninject;
using Artemis.UI.Avalonia.Screens.Root.ViewModels;
using Artemis.UI.Avalonia.Shared.Ninject;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FluentAvalonia.Styling;
using Ninject;
using ReactiveUI;
using Splat.Ninject;

namespace Artemis.UI.Avalonia
{
    public class App : Application
    {
        private StandardKernel _kernel = null!;

        public override void Initialize()
        {
            InitializeNinject();
            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _kernel.Get<RootViewModel>()
                };
                AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().ForceNativeTitleBarToTheme(desktop.MainWindow, "Dark");
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void InitializeNinject()
        {
            _kernel = new StandardKernel();
            _kernel.Settings.InjectNonPublic = true;

            _kernel.Load<CoreModule>();
            _kernel.Load<UIModule>();
            _kernel.Load<SharedUIModule>();

            _kernel.UseNinjectDependencyResolver();
        }
    }
}