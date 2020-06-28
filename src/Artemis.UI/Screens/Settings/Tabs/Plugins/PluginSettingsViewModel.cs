using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Abstract;
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
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly IWindowManager _windowManager;

        public PluginSettingsViewModel(Plugin plugin, IWindowManager windowManager, IDialogService dialogService, IPluginService pluginService,
            ISnackbarMessageQueue snackbarMessageQueue)
        {
            Plugin = plugin;
            PluginInfo = plugin.PluginInfo;

            _windowManager = windowManager;
            _dialogService = dialogService;
            _pluginService = pluginService;
            _snackbarMessageQueue = snackbarMessageQueue;
        }

        public Plugin Plugin { get; set; }
        public PluginInfo PluginInfo { get; set; }
        public bool Enabling { get; set; }
        public PackIconKind Icon => GetIconKind();
        public string Type => Plugin.GetType().BaseType?.Name ?? Plugin.GetType().Name;
        public bool CanOpenSettings => IsEnabled && Plugin.HasConfigurationViewModel;
        public bool DisplayLoadFailed => !Enabling && PluginInfo.LoadException != null;

        public bool IsEnabled
        {
            get => Plugin.Enabled;
            set => Task.Run(() => UpdateEnabled(value));
        }

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

        public async Task ShowLogsFolder()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));
            }
            catch (Exception e)
            {
                await _dialogService.ShowExceptionDialog("Welp, we couldn\'t open the logs folder for you", e);
            }
        }

        public async Task ShowLoadException()
        {
            if (PluginInfo.LoadException == null)
                return;

            await _dialogService.ShowExceptionDialog("The plugin failed to load: " + PluginInfo.LoadException.Message, PluginInfo.LoadException);
        }

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
                case BaseDataModelExpansion _:
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

        private async Task UpdateEnabled(bool enable)
        {
            if (Plugin.Enabled == enable)
            {
                NotifyOfPropertyChange(nameof(IsEnabled));
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
                    NotifyOfPropertyChange(nameof(IsEnabled));
                    return;
                }
            }

            if (enable)
            {
                Enabling = true;
                NotifyOfPropertyChange(nameof(DisplayLoadFailed));

                try
                {
                    _pluginService.EnablePlugin(Plugin);
                }
                catch (Exception e)
                {
                    _snackbarMessageQueue.Enqueue($"Failed to enable plugin {PluginInfo.Name}\r\n{e.Message}", "VIEW LOGS", async () => await ShowLogsFolder());
                }
                finally
                {
                    Enabling = false;
                    NotifyOfPropertyChange(nameof(IsEnabled));
                    NotifyOfPropertyChange(nameof(DisplayLoadFailed));
                }
            }
            else
                _pluginService.DisablePlugin(Plugin);

            NotifyOfPropertyChange(nameof(IsEnabled));
        }
    }
}