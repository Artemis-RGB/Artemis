using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.MacOS
{
    public class App : Application
    {
        public override void Initialize()
        {
            ArtemisBootstrapper.Bootstrap();
            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) 
                ArtemisBootstrapper.ConfigureApplicationLifetime(desktop);

            base.OnFrameworkInitializationCompleted();
        }
    }
}