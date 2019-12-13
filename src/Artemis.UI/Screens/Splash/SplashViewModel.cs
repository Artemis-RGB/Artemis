using System.Windows.Input;
using Artemis.Core.Services.Interfaces;
using MaterialDesignExtensions.Controls;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens.Splash
{
    public class SplashViewModel : Screen
    {
        private readonly IKernel _kernel;

        public SplashViewModel(IKernel kernel)
        {
            _kernel = kernel;
            Status = "Initializing Core";
        }

        public string Status { get; set; }

        public void ListenToEvents()
        {
            var pluginService = _kernel.Get<IPluginService>();
            pluginService.CopyingBuildInPlugins += (sender, args) => Status = "Updating built-in plugins";
            pluginService.PluginLoading += (sender, args) => Status = "Loading plugin: " + args.PluginInfo.Name;
            pluginService.PluginLoaded += (sender, args) => Status = "Initializing UI";
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the window by clicking anywhere
            if (e.ChangedButton == MouseButton.Left)
                ((MaterialWindow) View).DragMove();
        }
    }
}