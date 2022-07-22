using Artemis.Core.Services;
using Artemis.UI.Windows.Ninject;
using Artemis.UI.Windows.Providers.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Windows
{
    public class App : Application
    {
        public override void Initialize()
        {
            _kernel = ArtemisBootstrapper.Bootstrap(this, new WindowsModule());
            Program.CreateLogger(_kernel);
            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || Design.IsDesignMode)
                return;

            ArtemisBootstrapper.Initialize();
            _applicationStateManager = new ApplicationStateManager(_kernel!, desktop.Args);
            RegisterProviders(_kernel!);
        }

        private void RegisterProviders(StandardKernel standardKernel)
        {
            IInputService inputService = standardKernel.Get<IInputService>();
            inputService.AddInputProvider(standardKernel.Get<WindowsInputProvider>());
        }

        private StandardKernel? _kernel;

        // ReSharper disable NotAccessedField.Local
        private ApplicationStateManager? _applicationStateManager;
        // ReSharper restore NotAccessedField.Local
    }
}