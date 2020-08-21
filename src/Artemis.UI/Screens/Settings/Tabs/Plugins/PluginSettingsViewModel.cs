using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Shared.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsViewModel : PropertyChangedBase
    {
        private readonly IDialogService _dialogService;
        private readonly IPluginService _pluginService;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly IKernel _kernel;
        private readonly IWindowManager _windowManager;
        private Plugin _plugin;
        private PluginInfo _pluginInfo;
        private bool _enabling;

        public PluginSettingsViewModel(Plugin plugin,
            IKernel kernel,
            IWindowManager windowManager,
            IDialogService dialogService,
            IPluginService pluginService,
            ISnackbarMessageQueue snackbarMessageQueue)
        {
            Plugin = plugin;
            PluginInfo = plugin.PluginInfo;

            _kernel = kernel;
            _windowManager = windowManager;
            _dialogService = dialogService;
            _pluginService = pluginService;
            _snackbarMessageQueue = snackbarMessageQueue;
        }

        public Plugin Plugin
        {
            get => _plugin;
            set => SetAndNotify(ref _plugin, value);
        }

        public PluginInfo PluginInfo
        {
            get => _pluginInfo;
            set => SetAndNotify(ref _pluginInfo, value);
        }

        public bool Enabling
        {
            get => _enabling;
            set => SetAndNotify(ref _enabling, value);
        }

        public PackIconKind Icon => GetIconKind();
        public string Type => Plugin.GetType().BaseType?.Name ?? Plugin.GetType().Name;
        public bool CanOpenSettings => IsEnabled && Plugin.ConfigurationDialog != null;
        public bool DisplayLoadFailed => !Enabling && PluginInfo.LoadException != null;

        public bool IsEnabled
        {
            get => Plugin.Enabled;
            set => Task.Run(() => UpdateEnabled(value));
        }

        public void OpenSettings()
        {
            var configurationViewModel = Plugin.ConfigurationDialog;
            if (configurationViewModel == null)
                return;

            try
            {
                var plugin = new ConstructorArgument("plugin", Plugin);
                var viewModel = (PluginConfigurationViewModel) _kernel.Get(configurationViewModel.Type, plugin);
                _windowManager.ShowDialog(new PluginSettingsWindowViewModel(viewModel, Icon));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("An exception occured while trying to show the plugin's settings window", e);
                throw;
            }
        }

        public void ShowLogsFolder()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("Welp, we couldn\'t open the logs folder for you", e);
            }
        }

        public void ShowLoadException()
        {
            if (PluginInfo.LoadException == null)
                return;

            _dialogService.ShowExceptionDialog("The plugin failed to load: " + PluginInfo.LoadException.Message, PluginInfo.LoadException);
        }

        private PackIconKind GetIconKind()
        {
            if (PluginInfo.Icon != null)
            {
                var parsedIcon = Enum.TryParse<PackIconKind>(PluginInfo.Icon, true, out var iconEnum);
                if (parsedIcon)
                    return iconEnum;
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
                    _snackbarMessageQueue.Enqueue($"Failed to enable plugin {PluginInfo.Name}\r\n{e.Message}", "VIEW LOGS", ShowLogsFolder);
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