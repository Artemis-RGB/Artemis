using System;
using System.Windows.Input;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using MaterialDesignExtensions.Controls;
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
        }

        public string Status { get; set; }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the window by clicking anywhere
            if (e.ChangedButton == MouseButton.Left)
                ((MaterialWindow) View).DragMove();
        }

        protected override void OnInitialActivate()
        {
            _coreService.Initialized += OnCoreServiceOnInitialized;
            _pluginService.CopyingBuildInPlugins += OnPluginServiceOnCopyingBuildInPlugins;
            _pluginService.PluginLoading += OnPluginServiceOnPluginLoading;
            _pluginService.PluginLoaded += OnPluginServiceOnPluginLoaded;
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _coreService.Initialized -= OnCoreServiceOnInitialized;
            _pluginService.CopyingBuildInPlugins -= OnPluginServiceOnCopyingBuildInPlugins;
            _pluginService.PluginLoading -= OnPluginServiceOnPluginLoading;
            _pluginService.PluginLoaded -= OnPluginServiceOnPluginLoaded;
            base.OnClose();
        }
        
        private void OnPluginServiceOnPluginLoaded(object sender, PluginEventArgs args)
        {
            Status = "Initializing UI";
        }

        private void OnPluginServiceOnPluginLoading(object sender, PluginEventArgs args)
        {
            Status = "Loading plugin: " + args.PluginInfo.Name;
        }

        private void OnPluginServiceOnCopyingBuildInPlugins(object sender, EventArgs args)
        {
            Status = "Updating built-in plugins";
        }

        private void OnCoreServiceOnInitialized(object sender, EventArgs args)
        {
            Execute.OnUIThread(() => RequestClose());
        }
    }
}