using System;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Shared.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsViewModel : PropertyChangedBase
    {
        private readonly IDialogService _dialogService;
        private readonly Plugin _plugin;
        private readonly IPluginService _pluginService;
        private readonly IWindowManager _windowManager;

        public PluginSettingsViewModel(Plugin plugin, IWindowManager windowManager, IDialogService dialogService, IPluginService pluginService)
        {
            _plugin = plugin;
            _windowManager = windowManager;
            _dialogService = dialogService;
            _pluginService = pluginService;
        }

        public string Type => _plugin.GetType().BaseType?.Name ?? _plugin.GetType().Name;
        public string Name => _plugin.PluginInfo.Name;
        public string Description => _plugin.PluginInfo.Description;
        public Version Version => _plugin.PluginInfo.Version;

        public bool IsEnabled
        {
            get => _plugin.PluginInfo.Enabled;
            set => Task.Run(() => UpdateEnabled(value));
        }

        public bool CanOpenSettings => IsEnabled && _plugin.HasConfigurationViewModel;

        public async Task OpenSettings()
        {
            try
            {
                var configurationViewModel = _plugin.GetConfigurationViewModel();
                if (configurationViewModel != null)
                    _windowManager.ShowDialog(new PluginSettingsWindowViewModel(configurationViewModel));
            }
            catch (Exception e)
            {
                await _dialogService.ShowExceptionDialog("An exception occured while trying to show the plugin's settings window", e);
                throw;
            }
        }

        private void UpdateEnabled(in bool enable)
        {
            if (_plugin.PluginInfo.Enabled == enable)
                return;

            if (enable)
                _pluginService.EnablePlugin(_plugin);
            else
                _pluginService.DisablePlugin(_plugin);
        }
    }
}