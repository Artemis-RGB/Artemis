using System;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using MaterialDesignExtensions.Controls;
using Stylet;

namespace Artemis.UI.Screens.Splash
{
    public class SplashViewModel : Screen
    {
        private readonly ICoreService _coreService;
        private readonly IPluginManagementService _pluginManagementService;
        private string _status;

        public SplashViewModel(ICoreService coreService, IPluginManagementService pluginManagementService)
        {
            _coreService = coreService;
            _pluginManagementService = pluginManagementService;
            Status = "Initializing Core";
        }

        public string Status
        {
            get => _status;
            set => SetAndNotify(ref _status, value);
        }

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
            _pluginManagementService.CopyingBuildInPlugins += OnPluginManagementServiceOnCopyingBuildInPluginsManagement;
            _pluginManagementService.PluginLoading += OnPluginManagementServiceOnPluginManagementLoading;
            _pluginManagementService.PluginLoaded += OnPluginManagementServiceOnPluginManagementLoaded;
            _pluginManagementService.PluginEnabling += PluginManagementServiceOnPluginManagementEnabling;
            _pluginManagementService.PluginEnabled += PluginManagementServiceOnPluginManagementEnabled;
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _coreService.Initialized -= OnCoreServiceOnInitialized;
            _pluginManagementService.CopyingBuildInPlugins -= OnPluginManagementServiceOnCopyingBuildInPluginsManagement;
            _pluginManagementService.PluginLoading -= OnPluginManagementServiceOnPluginManagementLoading;
            _pluginManagementService.PluginLoaded -= OnPluginManagementServiceOnPluginManagementLoaded;
            _pluginManagementService.PluginEnabling -= PluginManagementServiceOnPluginManagementEnabling;
            _pluginManagementService.PluginEnabled -= PluginManagementServiceOnPluginManagementEnabled;
            base.OnClose();
        }

        private void OnPluginManagementServiceOnPluginManagementLoaded(object sender, PluginEventArgs args)
        {
            Status = "Initializing UI";
        }

        private void OnPluginManagementServiceOnPluginManagementLoading(object sender, PluginEventArgs args)
        {
            Status = "Loading plugin: " + args.PluginInfo.Name;
        }

        private void PluginManagementServiceOnPluginManagementEnabled(object sender, PluginEventArgs args)
        {
            Status = "Initializing UI";
        }

        private void PluginManagementServiceOnPluginManagementEnabling(object sender, PluginEventArgs args)
        {
            Status = "Enabling plugin: " + args.PluginInfo.Name;
        }

        private void OnPluginManagementServiceOnCopyingBuildInPluginsManagement(object sender, EventArgs args)
        {
            Status = "Updating built-in plugins";
        }

        private void OnCoreServiceOnInitialized(object sender, EventArgs args)
        {
            Execute.OnUIThread(() => RequestClose());
        }
    }
}