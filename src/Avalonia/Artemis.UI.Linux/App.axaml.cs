using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Linux
{
    public class App : Application
    {
        public override void Initialize()
        {
            ArtemisBootstrapper.Bootstrap(this);
            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ArtemisBootstrapper.Initialize();
        }
    }
}