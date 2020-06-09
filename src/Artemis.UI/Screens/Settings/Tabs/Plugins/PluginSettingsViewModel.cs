using System;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Shared.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsViewModel : PropertyChangedBase
    {
        private readonly IDialogService _dialogService;
        private readonly IPluginService _pluginService;
        private readonly IWindowManager _windowManager;

        public PluginSettingsViewModel(Plugin plugin, IWindowManager windowManager, IDialogService dialogService, IPluginService pluginService)
        {
            Plugin = plugin;
            PluginInfo = plugin.PluginInfo;

            _windowManager = windowManager;
            _dialogService = dialogService;
            _pluginService = pluginService;
        }

        public Plugin Plugin { get; set; }
        public PluginInfo PluginInfo { get; set; }

        public string Type => Plugin.GetType().BaseType?.Name ?? Plugin.GetType().Name;

        public PackIconKind Icon => GetIconKind();

        private PackIconKind GetIconKind()
        {
            if (PluginInfo.Icon != null)
            {
                var parsedIcon = Enum.TryParse<PackIconKind>(PluginInfo.Icon, true, out var iconEnum);
                if (parsedIcon == false)
                    return PackIconKind.QuestionMarkCircle;
            }

            switch (Plugin)
            {
                case DataModelExpansion _:
                    return PackIconKind.TableAdd;
                case DeviceProvider _:
                    return PackIconKind.Devices;
                case ProfileModule _:
                    return PackIconKind.VectorRectangle;
                case Core.Plugins.Abstract.Module _:
                    return PackIconKind.GearBox;
                case LayerBrushProvider _:
                    return PackIconKind.Brush;        
                case LayerEffectProvider _:
                    return PackIconKind.AutoAwesome;
            }

            return PackIconKind.Plugin;
        }

        public bool IsEnabled
        {
            get => PluginInfo.Enabled;
            set => Task.Run(() => UpdateEnabled(value));
        }

        public bool CanOpenSettings => IsEnabled && Plugin.HasConfigurationViewModel;

        public async Task OpenSettings()
        {
            try
            {
                var configurationViewModel = Plugin.GetConfigurationViewModel();
                if (configurationViewModel != null)
                    _windowManager.ShowDialog(new PluginSettingsWindowViewModel(configurationViewModel));
            }
            catch (Exception e)
            {
                await _dialogService.ShowExceptionDialog("An exception occured while trying to show the plugin's settings window", e);
                throw;
            }
        }

        private async Task UpdateEnabled(bool enable)
        {
            if (PluginInfo.Enabled == enable)
            {
                NotifyOfPropertyChange(() => IsEnabled);
                return;
            }

            if (!enable && Plugin is DeviceProvider)
            {
                var confirm = await _dialogService.ShowConfirmDialog(
                    "Disable device provider",
                    "You are disabling a device provider, this requires that Artemis restarts, please confirm."
                );
                if (!confirm)
                {
                    NotifyOfPropertyChange(() => IsEnabled);
                    return;
                }
            }

            if (enable)
                _pluginService.EnablePlugin(Plugin);
            else
                _pluginService.DisablePlugin(Plugin);

            NotifyOfPropertyChange(() => IsEnabled);
        }
    }
}