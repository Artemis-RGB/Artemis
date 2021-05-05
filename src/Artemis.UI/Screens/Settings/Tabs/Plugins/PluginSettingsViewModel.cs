using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsViewModel : Conductor<PluginFeatureViewModel>.Collection.AllActive
    {
        private readonly ICoreService _coreService;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly ISettingsVmFactory _settingsVmFactory;
        private readonly IWindowManager _windowManager;
        private bool _canInstallPrerequisites;
        private bool _canRemovePrerequisites;
        private bool _enabling;
        private bool _isSettingsPopupOpen;
        private Plugin _plugin;

        public PluginSettingsViewModel(Plugin plugin,
            ISettingsVmFactory settingsVmFactory,
            ICoreService coreService,
            IWindowManager windowManager,
            IDialogService dialogService,
            IPluginManagementService pluginManagementService,
            IMessageService messageService)
        {
            Plugin = plugin;

            _settingsVmFactory = settingsVmFactory;
            _coreService = coreService;
            _windowManager = windowManager;
            _dialogService = dialogService;
            _pluginManagementService = pluginManagementService;
            _messageService = messageService;

            Icon = PluginUtilities.GetPluginIcon(Plugin, Plugin.Info.Icon);
        }

        public Plugin Plugin
        {
            get => _plugin;
            set => SetAndNotify(ref _plugin, value);
        }

        public bool Enabling
        {
            get => _enabling;
            set => SetAndNotify(ref _enabling, value);
        }

        public object Icon { get; set; }
        public string Type => Plugin.GetType().BaseType?.Name ?? Plugin.GetType().Name;
        public bool CanOpenSettings => IsEnabled && Plugin.ConfigurationDialog != null;

        public bool IsEnabled
        {
            get => Plugin.IsEnabled;
            set => Task.Run(() => UpdateEnabled(value));
        }

        public bool IsSettingsPopupOpen
        {
            get => _isSettingsPopupOpen;
            set
            {
                if (!SetAndNotify(ref _isSettingsPopupOpen, value)) return;
                CheckPrerequisites();
            }
        }

        public bool CanInstallPrerequisites
        {
            get => _canInstallPrerequisites;
            set => SetAndNotify(ref _canInstallPrerequisites, value);
        }

        public bool CanRemovePrerequisites
        {
            get => _canRemovePrerequisites;
            set => SetAndNotify(ref _canRemovePrerequisites, value);
        }

        public void OpenSettings()
        {
            PluginConfigurationDialog configurationViewModel = (PluginConfigurationDialog) Plugin.ConfigurationDialog;
            if (configurationViewModel == null)
                return;

            try
            {
                PluginConfigurationViewModel viewModel = (PluginConfigurationViewModel) Plugin.Kernel.Get(configurationViewModel.Type);
                _windowManager.ShowWindow(new PluginSettingsWindowViewModel(viewModel, Icon));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("An exception occured while trying to show the plugin's settings window", e);
                throw;
            }
        }

        public void OpenPluginDirectory()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Plugin.Directory.FullName);
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("Welp, we couldn't open the device's plugin folder for you", e);
            }
        }

        public async Task Reload()
        {
            bool wasEnabled = IsEnabled;

            _pluginManagementService.UnloadPlugin(Plugin);
            Items.Clear();

            Plugin = _pluginManagementService.LoadPlugin(Plugin.Directory);
            foreach (PluginFeatureInfo pluginFeatureInfo in Plugin.Features)
                Items.Add(_settingsVmFactory.CreatePluginFeatureViewModel(pluginFeatureInfo, false));

            if (wasEnabled)
                await UpdateEnabled(true);

            _messageService.ShowMessage("Reloaded plugin.");
        }

        public async Task InstallPrerequisites()
        {
            List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
            subjects.AddRange(Plugin.Features.Where(f => f.AlwaysEnabled));

            if (subjects.Any(s => s.Prerequisites.Any()))
                await PluginPrerequisitesInstallDialogViewModel.Show(_dialogService, subjects);
        }

        public async Task RemovePrerequisites(bool forPluginRemoval = false)
        {
            List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
            subjects.AddRange(!forPluginRemoval ? Plugin.Features.Where(f => f.AlwaysEnabled) : Plugin.Features);

            if (subjects.Any(s => s.Prerequisites.Any(p => p.UninstallActions.Any())))
            {
                await PluginPrerequisitesUninstallDialogViewModel.Show(_dialogService, subjects, forPluginRemoval ? "SKIP, REMOVE PLUGIN" : "CANCEL");
                NotifyOfPropertyChange(nameof(IsEnabled));
                NotifyOfPropertyChange(nameof(CanOpenSettings));
            }
        }

        public async Task RemoveSettings()
        {
            bool confirmed = await _dialogService.ShowConfirmDialog("Clear plugin settings", "Are you sure you want to clear the settings of this plugin?");
            if (!confirmed)
                return;

            bool wasEnabled = IsEnabled;

            if (IsEnabled)
                await UpdateEnabled(false);

            _pluginManagementService.RemovePluginSettings(Plugin);

            if (wasEnabled)
                await UpdateEnabled(true);

            _messageService.ShowMessage("Cleared plugin settings.");
        }

        public async Task Remove()
        {
            bool confirmed = await _dialogService.ShowConfirmDialog("Remove plugin", "Are you sure you want to remove this plugin?");
            if (!confirmed)
                return;

            // If the plugin or any of its features has uninstall actions, offer to run these
            List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
            subjects.AddRange(Plugin.Features);
            if (subjects.Any(s => s.Prerequisites.Any(p => p.UninstallActions.Any())))
                await RemovePrerequisites(true);

            try
            {
                _pluginManagementService.RemovePlugin(Plugin, false);
                ((PluginSettingsTabViewModel) Parent).GetPluginInstances();
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("Failed to remove plugin", e);
                throw;
            }

            _messageService.ShowMessage("Removed plugin.");
        }

        public void ShowLogsFolder()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Path.Combine(Constants.DataFolder, "Logs"));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("Welp, we couldn\'t open the logs folder for you", e);
            }
        }

        private void PluginManagementServiceOnPluginToggled(object? sender, PluginEventArgs e)
        {
            NotifyOfPropertyChange(nameof(IsEnabled));
            NotifyOfPropertyChange(nameof(CanOpenSettings));
        }

        private async Task UpdateEnabled(bool enable)
        {
            if (IsEnabled == enable)
            {
                NotifyOfPropertyChange(nameof(IsEnabled));
                return;
            }

            if (enable)
            {
                Enabling = true;

                if (Plugin.Info.RequiresAdmin && !_coreService.IsElevated)
                {
                    bool confirmed = await _dialogService.ShowConfirmDialog("Enable plugin", "This plugin requires admin rights, are you sure you want to enable it?");
                    if (!confirmed)
                    {
                        CancelEnable();
                        return;
                    }
                }

                // Check if all prerequisites are met async
                List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
                subjects.AddRange(Plugin.Features.Where(f => f.AlwaysEnabled || f.EnabledInStorage));

                if (subjects.Any(s => !s.ArePrerequisitesMet()))
                {
                    await PluginPrerequisitesInstallDialogViewModel.Show(_dialogService, subjects);
                    if (!subjects.All(s => s.ArePrerequisitesMet()))
                    {
                        CancelEnable();
                        return;
                    }
                }

                await Task.Run(() =>
                {
                    try
                    {
                        _pluginManagementService.EnablePlugin(Plugin, true, true);
                    }
                    catch (Exception e)
                    {
                        _messageService.ShowMessage($"Failed to enable plugin {Plugin.Info.Name}\r\n{e.Message}", "VIEW LOGS", ShowLogsFolder);
                    }
                    finally
                    {
                        Enabling = false;
                    }
                });
            }
            else
                _pluginManagementService.DisablePlugin(Plugin, true);

            NotifyOfPropertyChange(nameof(IsEnabled));
            NotifyOfPropertyChange(nameof(CanOpenSettings));
        }

        private void CancelEnable()
        {
            Enabling = false;
            NotifyOfPropertyChange(nameof(IsEnabled));
            NotifyOfPropertyChange(nameof(CanOpenSettings));
        }

        private void CheckPrerequisites()
        {
            CanInstallPrerequisites = Plugin.Info.Prerequisites.Any() ||
                                      Plugin.Features.Where(f => f.AlwaysEnabled).Any(f => f.Prerequisites.Any());
            CanRemovePrerequisites = Plugin.Info.Prerequisites.Any(p => p.UninstallActions.Any()) ||
                                     Plugin.Features.Where(f => f.AlwaysEnabled).Any(f => f.Prerequisites.Any(p => p.UninstallActions.Any()));
        }

        #region Overrides of Screen

        protected override void OnInitialActivate()
        {
            foreach (PluginFeatureInfo pluginFeatureInfo in Plugin.Features)
                Items.Add(_settingsVmFactory.CreatePluginFeatureViewModel(pluginFeatureInfo, false));

            _pluginManagementService.PluginDisabled += PluginManagementServiceOnPluginToggled;
            _pluginManagementService.PluginEnabled += PluginManagementServiceOnPluginToggled;
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _pluginManagementService.PluginDisabled -= PluginManagementServiceOnPluginToggled;
            _pluginManagementService.PluginEnabled -= PluginManagementServiceOnPluginToggled;
            base.OnClose();
        }

        #endregion
    }
}