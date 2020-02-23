using System.Windows.Input;
using Artemis.Core.Services.Interfaces;
using MaterialDesignExtensions.Controls;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens.Splash
{
    public class SplashViewModel : Screen
    {
        private readonly ICoreService _coreService;
        private readonly IPluginService _pluginService;

        public SplashViewModel(ICoreService coreService, IPluginService pluginService)
        {
            _coreService = coreService;
            _pluginService = pluginService;
            Status = "Initializing Core";

            ListenToEvents();
        }

        public string Status { get; set; }

        public void ListenToEvents()
        {
            _coreService.Initialized += (sender, args) => Execute.OnUIThread(() => RequestClose());
            _pluginService.CopyingBuildInPlugins += (sender, args) => Status = "Updating built-in plugins";
            _pluginService.PluginLoading += (sender, args) => Status = "Loading plugin: " + args.PluginInfo.Name;
            _pluginService.PluginLoaded += (sender, args) => Status = "Initializing UI";
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