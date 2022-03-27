using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using Avalonia.Threading;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins
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

           

            OpenSettings = ReactiveCommand.Create(ExecuteOpenSettings, this.WhenAnyValue(x => x.IsEnabled).Select(isEnabled => isEnabled && Plugin.ConfigurationDialog != null));
            InstallPrerequisites = ReactiveCommand.CreateFromTask(ExecuteInstallPrerequisites, this.WhenAnyValue(x => x.CanInstallPrerequisites));
            RemovePrerequisites = ReactiveCommand.CreateFromTask<bool>(ExecuteRemovePrerequisites, this.WhenAnyValue(x => x.CanRemovePrerequisites));

            this.WhenActivated(d =>
            {
                _pluginManagementService.PluginDisabled += PluginManagementServiceOnPluginToggled;
                _pluginManagementService.PluginEnabled += PluginManagementServiceOnPluginToggled;

                Disposable.Create(() =>
                {
                    _pluginManagementService.PluginDisabled -= PluginManagementServiceOnPluginToggled;
                    _pluginManagementService.PluginEnabled -= PluginManagementServiceOnPluginToggled;
                }).DisposeWith(d);
            });
        }

        public ReactiveCommand<Unit, Unit> OpenSettings { get; }
        public ReactiveCommand<Unit, Unit> InstallPrerequisites { get; }
        public ReactiveCommand<bool, Unit> RemovePrerequisites { get; }

        public ObservableCollection<PluginFeatureViewModel> PluginFeatures { get; }

        public Plugin Plugin
        {
            get => _plugin;
            set => RaiseAndSetIfChanged(ref _plugin, value);
        }

        public bool Enabling
        {
            get => _enabling;
            set => RaiseAndSetIfChanged(ref _enabling, value);
        }

        public string Type => Plugin.GetType().BaseType?.Name ?? Plugin.GetType().Name;

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
                if (!RaiseAndSetIfChanged(ref _isSettingsPopupOpen, value)) return;
                CheckPrerequisites();
            }
        }

        public bool CanInstallPrerequisites
        {
            get => _canInstallPrerequisites;
            set => RaiseAndSetIfChanged(ref _canInstallPrerequisites, value);
        }

        public bool CanRemovePrerequisites
        {
            get => _canRemovePrerequisites;
            set => RaiseAndSetIfChanged(ref _canRemovePrerequisites, value);
        }

        private void ExecuteOpenSettings()
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

            await Task.Run(() => _pluginManagementService.UnloadPlugin(Plugin));

            PluginFeatures.Clear();

            Plugin = _pluginManagementService.LoadPlugin(Plugin.Directory);
            foreach (PluginFeatureInfo pluginFeatureInfo in Plugin.Features)
                PluginFeatures.Add(_settingsVmFactory.CreatePluginFeatureViewModel(pluginFeatureInfo, false));

            if (wasEnabled)
                await UpdateEnabled(true);

            _notificationService.CreateNotification().WithTitle("Reloaded plugin.").Show();
        }

        public async Task ExecuteInstallPrerequisites()
        {
            List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
            subjects.AddRange(Plugin.Features.Where(f => f.AlwaysEnabled));

            if (subjects.Any(s => s.Prerequisites.Any()))
                await PluginPrerequisitesInstallDialogViewModel.Show(_windowService, subjects);
        }

        public async Task ExecuteRemovePrerequisites(bool forPluginRemoval = false)
        {
            List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
            subjects.AddRange(!forPluginRemoval ? Plugin.Features.Where(f => f.AlwaysEnabled) : Plugin.Features);

            if (subjects.Any(s => s.Prerequisites.Any(p => p.UninstallActions.Any())))
            {
                await PluginPrerequisitesUninstallDialogViewModel.Show(_windowService, subjects, forPluginRemoval ? "Skip, remove plugin" : "Cancel");
                this.RaisePropertyChanged(nameof(IsEnabled));
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
                await ExecuteRemovePrerequisites(true);

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
                Utilities.OpenFolder(Constants.LogsFolder);
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

        private void PluginManagementServiceOnPluginToggled(object? sender, PluginEventArgs e)
        {
            this.RaisePropertyChanged(nameof(IsEnabled));
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

                await Task.Run(async () =>
                {
                    try
                    {
                        _pluginManagementService.EnablePlugin(Plugin, true, true);
                    }
                    catch (Exception e)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => _notificationService.CreateNotification()
                            .WithMessage($"Failed to enable plugin {Plugin.Info.Name}\r\n{e.Message}")
                            .HavingButton(b => b.WithText("View logs").WithAction(ShowLogsFolder))
                            .Show());
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
        }

        private void CancelEnable()
        {
            Enabling = false;
            this.RaisePropertyChanged(nameof(IsEnabled));
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