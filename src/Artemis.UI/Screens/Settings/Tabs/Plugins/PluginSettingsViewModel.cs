using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.DeviceProviders;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Ninject;
using Ninject.Parameters;
using Serilog;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsViewModel : PropertyChangedBase
    {
        private readonly IDialogService _dialogService;
        private readonly IKernel _kernel;
        private readonly ILogger _logger;
        private readonly IPluginService _pluginService;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly IWindowManager _windowManager;
        private bool _enabling;
        private Plugin _plugin;
        private PluginInfo _pluginInfo;

        public PluginSettingsViewModel(Plugin plugin,
            IKernel kernel,
            ILogger logger,
            IWindowManager windowManager,
            IDialogService dialogService,
            IPluginService pluginService,
            ISnackbarMessageQueue snackbarMessageQueue)
        {
            Plugin = plugin;
            PluginInfo = plugin.PluginInfo;

            _kernel = kernel;
            _logger = logger;
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
        public bool RequiresRestart => Plugin.Enabled && !PluginInfo.Enabled;

        public bool IsEnabled
        {
            get => Plugin.PluginInfo.Enabled;
            set => Task.Run(() => UpdateEnabled(value));
        }

        public void OpenSettings()
        {
            var configurationViewModel = Plugin.ConfigurationDialog;
            if (configurationViewModel == null)
                return;

            try
            {
                // Limit to one constructor, there's no need to have more and it complicates things anyway
                var constructors = configurationViewModel.Type.GetConstructors();
                if (constructors.Length != 1)
                    throw new ArtemisUIException("Plugin configuration dialogs must have exactly one constructor");

                var pluginParameter = constructors.First().GetParameters().First(p => typeof(Plugin).IsAssignableFrom(p.ParameterType));
                var plugin = new ConstructorArgument(pluginParameter.Name, Plugin);
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

        public async Task Restart()
        {
            _logger.Debug("Restarting for device provider disable {pluginInfo}", Plugin.PluginInfo);

            // Give the logger a chance to write, might not always be enough but oh well
            await Task.Delay(500);
            ApplicationUtilities.Shutdown(2, true);
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
                case Module _:
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
            if (IsEnabled == enable)
            {
                NotifyOfPropertyChange(nameof(IsEnabled));
                return;
            }

            if (!enable && Plugin is DeviceProvider)
            {
                await DisableDeviceProvider();
                return;
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
                }
            }
            else
                _pluginService.DisablePlugin(Plugin);

            NotifyOfPropertyChange(nameof(IsEnabled));
            NotifyOfPropertyChange(nameof(RequiresRestart));
            NotifyOfPropertyChange(nameof(DisplayLoadFailed));
        }

        private async Task DisableDeviceProvider()
        {
            var restart = false;

            // If any plugin already requires a restart, don't ask the user again
            var restartQueued = _pluginService.GetAllPluginInfo().Any(p => p.Instance != null && !p.Enabled && p.Instance.Enabled);
            // If the plugin isn't enabled (load failed), it can be disabled without a restart
            if (!restartQueued && Plugin.Enabled)
            {
                restart = await _dialogService.ShowConfirmDialog(
                    "Disable device provider",
                    "You are disabling a device provider, Artemis has to restart to \r\nfully disable this type of plugin",
                    "Restart now",
                    "Restart later"
                );
            }

            _pluginService.DisablePlugin(Plugin);
            if (restart)
            {
                _logger.Debug("Restarting for device provider disable {pluginInfo}", Plugin.PluginInfo);

                // Give the logger a chance to write, might not always be enough but oh well
                await Task.Delay(500);
                ApplicationUtilities.Shutdown(2, true);
            }

            NotifyOfPropertyChange(nameof(IsEnabled));
            NotifyOfPropertyChange(nameof(RequiresRestart));
            NotifyOfPropertyChange(nameof(DisplayLoadFailed));
        }
    }
}