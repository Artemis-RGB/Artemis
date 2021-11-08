using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Avalonia.Exceptions;
using Artemis.UI.Avalonia.Ninject.Factories;
using Artemis.UI.Avalonia.Shared;
using Artemis.UI.Avalonia.Shared.Services.Interfaces;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Plugins.ViewModels
{
    public class PluginSettingsViewModel : ActivatableViewModelBase
    {
        private readonly ICoreService _coreService;
        private readonly INotificationService _notificationService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly ISettingsVmFactory _settingsVmFactory;
        private readonly IWindowService _windowService;
        private bool _canInstallPrerequisites;
        private bool _canRemovePrerequisites;
        private bool _enabling;
        private bool _isSettingsPopupOpen;
        private Plugin _plugin;

        public PluginSettingsViewModel(Plugin plugin,
            ISettingsVmFactory settingsVmFactory,
            ICoreService coreService,
            IWindowService windowService,
            INotificationService notificationService,
            IPluginManagementService pluginManagementService)
        {
            _plugin = plugin;

            _settingsVmFactory = settingsVmFactory;
            _coreService = coreService;
            _windowService = windowService;
            _notificationService = notificationService;
            _pluginManagementService = pluginManagementService;

            PluginFeatures = new ObservableCollection<PluginFeatureViewModel>();
            foreach (PluginFeatureInfo pluginFeatureInfo in Plugin.Features)
                PluginFeatures.Add(_settingsVmFactory.CreatePluginFeatureViewModel(pluginFeatureInfo, false));

            _pluginManagementService.PluginDisabled += PluginManagementServiceOnPluginToggled;
            _pluginManagementService.PluginEnabled += PluginManagementServiceOnPluginToggled;
        }

        public ObservableCollection<PluginFeatureViewModel> PluginFeatures { get; }

        public Plugin Plugin
        {
            get => _plugin;
            set => this.RaiseAndSetIfChanged(ref _plugin, value);
        }

        public bool Enabling
        {
            get => _enabling;
            set => this.RaiseAndSetIfChanged(ref _enabling, value);
        }

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
                if (!this.RaiseAndSetIfChanged(ref _isSettingsPopupOpen, value)) return;
                CheckPrerequisites();
            }
        }

        public bool CanInstallPrerequisites
        {
            get => _canInstallPrerequisites;
            set => this.RaiseAndSetIfChanged(ref _canInstallPrerequisites, value);
        }

        public bool CanRemovePrerequisites
        {
            get => _canRemovePrerequisites;
            set => this.RaiseAndSetIfChanged(ref _canRemovePrerequisites, value);
        }

        public void OpenSettings()
        {
            if (Plugin.ConfigurationDialog == null)
                return;

            try
            {
                PluginConfigurationViewModel? viewModel = Plugin.Kernel!.Get(Plugin.ConfigurationDialog.Type) as PluginConfigurationViewModel;
                if (viewModel == null)
                    throw new ArtemisUIException($"The type of a plugin configuration dialog must inherit {nameof(PluginConfigurationViewModel)}");

                _windowService.ShowWindow(new PluginSettingsWindowViewModel(viewModel));
            }
            catch (Exception e)
            {
                _windowService.ShowExceptionDialog("An exception occured while trying to show the plugin's settings window", e);
                throw;
            }
        }

        public void OpenPluginDirectory()
        {
            try
            {
                Utilities.OpenFolder(Plugin.Directory.FullName);
            }
            catch (Exception e)
            {
                _windowService.ShowExceptionDialog("Welp, we couldn't open the device's plugin folder for you", e);
            }
        }

        public async Task Reload()
        {
            bool wasEnabled = IsEnabled;

            _pluginManagementService.UnloadPlugin(Plugin);
            PluginFeatures.Clear();

            Plugin = _pluginManagementService.LoadPlugin(Plugin.Directory);
            foreach (PluginFeatureInfo pluginFeatureInfo in Plugin.Features)
                PluginFeatures.Add(_settingsVmFactory.CreatePluginFeatureViewModel(pluginFeatureInfo, false));

            if (wasEnabled)
                await UpdateEnabled(true);

            _notificationService.CreateNotification().WithTitle("Reloaded plugin.").Show();
        }

        public async Task InstallPrerequisites()
        {
            List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
            subjects.AddRange(Plugin.Features.Where(f => f.AlwaysEnabled));

            if (subjects.Any(s => s.Prerequisites.Any()))
                await PluginPrerequisitesInstallDialogViewModel.Show(_windowService, subjects);
        }

        public async Task RemovePrerequisites(bool forPluginRemoval = false)
        {
            List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
            subjects.AddRange(!forPluginRemoval ? Plugin.Features.Where(f => f.AlwaysEnabled) : Plugin.Features);

            if (subjects.Any(s => s.Prerequisites.Any(p => p.UninstallActions.Any())))
            {
                await PluginPrerequisitesUninstallDialogViewModel.Show(_windowService, subjects, forPluginRemoval ? "Skip, remove plugin" : "Cancel");
                this.RaisePropertyChanged(nameof(IsEnabled));
                this.RaisePropertyChanged(nameof(CanOpenSettings));
            }
        }

        public async Task RemoveSettings()
        {
            bool confirmed = await _windowService.ShowConfirmContentDialog("Clear plugin settings", "Are you sure you want to clear the settings of this plugin?");
            if (!confirmed)
                return;

            bool wasEnabled = IsEnabled;

            if (IsEnabled)
                await UpdateEnabled(false);

            _pluginManagementService.RemovePluginSettings(Plugin);

            if (wasEnabled)
                await UpdateEnabled(true);

            _notificationService.CreateNotification().WithTitle("Cleared plugin settings.").Show();
        }

        public async Task Remove()
        {
            bool confirmed = await _windowService.ShowConfirmContentDialog("Remove plugin", "Are you sure you want to remove this plugin?");
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
            }
            catch (Exception e)
            {
                _windowService.ShowExceptionDialog("Failed to remove plugin", e);
                throw;
            }

            _notificationService.CreateNotification().WithTitle("Removed plugin.").Show();
        }

        public void ShowLogsFolder()
        {
            try
            {
                Utilities.OpenFolder(Path.Combine(Constants.DataFolder, "logs"));
            }
            catch (Exception e)
            {
                _windowService.ShowExceptionDialog("Welp, we couldn\'t open the logs folder for you", e);
            }
        }

        public void OpenUri(Uri uri)
        {
            Utilities.OpenUrl(uri.ToString());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pluginManagementService.PluginDisabled -= PluginManagementServiceOnPluginToggled;
                _pluginManagementService.PluginEnabled -= PluginManagementServiceOnPluginToggled;
            }

            base.Dispose(disposing);
        }

        private void PluginManagementServiceOnPluginToggled(object? sender, PluginEventArgs e)
        {
            this.RaisePropertyChanged(nameof(IsEnabled));
            this.RaisePropertyChanged(nameof(CanOpenSettings));
        }

        private async Task UpdateEnabled(bool enable)
        {
            if (IsEnabled == enable)
            {
                this.RaisePropertyChanged(nameof(IsEnabled));
                return;
            }

            if (enable)
            {
                Enabling = true;

                if (Plugin.Info.RequiresAdmin && !_coreService.IsElevated)
                {
                    bool confirmed = await _windowService.ShowConfirmContentDialog("Enable plugin", "This plugin requires admin rights, are you sure you want to enable it?");
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
                    await PluginPrerequisitesInstallDialogViewModel.Show(_windowService, subjects);
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
                        _notificationService.CreateNotification()
                            .WithMessage($"Failed to enable plugin {Plugin.Info.Name}\r\n{e.Message}")
                            .HavingButton(b => b.WithText("View logs").WithAction(ShowLogsFolder))
                            .Show();
                    }
                    finally
                    {
                        Enabling = false;
                    }
                });
            }
            else
            {
                _pluginManagementService.DisablePlugin(Plugin, true);
            }

            this.RaisePropertyChanged(nameof(IsEnabled));
            this.RaisePropertyChanged(nameof(CanOpenSettings));
        }

        private void CancelEnable()
        {
            Enabling = false;
            this.RaisePropertyChanged(nameof(IsEnabled));
            this.RaisePropertyChanged(nameof(CanOpenSettings));
        }

        private void CheckPrerequisites()
        {
            CanInstallPrerequisites = Plugin.Info.Prerequisites.Any() ||
                                      Plugin.Features.Where(f => f.AlwaysEnabled).Any(f => f.Prerequisites.Any());
            CanRemovePrerequisites = Plugin.Info.Prerequisites.Any(p => p.UninstallActions.Any()) ||
                                     Plugin.Features.Where(f => f.AlwaysEnabled).Any(f => f.Prerequisites.Any(p => p.UninstallActions.Any()));
        }
    }
}