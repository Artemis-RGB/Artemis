using Artemis.Core.Ninject;
using Artemis.UI.Avalonia.Ninject;
using Artemis.UI.Avalonia.Screens.Main.Views;
using Artemis.UI.Avalonia.Screens.Root.ViewModels;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Ninject;
using Splat.Ninject;

namespace Artemis.UI.Avalonia
{
    public class App : Application
    {
        private StandardKernel _kernel = null!;

        public override void Initialize()
        {
            InitializeNinject();
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _kernel.Get<RootViewModel>()
                };

            base.OnFrameworkInitializationCompleted();
        }

        private void InitializeNinject()
        {
            _kernel = new StandardKernel();
            _kernel.Load<CoreModule>();
            _kernel.Load<UIModule>();

            _kernel.UseNinjectDependencyResolver();
        }
    }
}