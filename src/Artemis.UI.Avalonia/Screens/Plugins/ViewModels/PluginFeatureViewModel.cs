using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Avalonia.Shared;
using Artemis.UI.Avalonia.Shared.Services.Builders;
using Artemis.UI.Avalonia.Shared.Services.Interfaces;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Plugins.ViewModels
{
    public class PluginFeatureViewModel : ActivatableViewModelBase
    {
        private readonly ICoreService _coreService;
        private readonly INotificationService _notificationService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IWindowService _windowService;
        private bool _enabling;

        public PluginFeatureViewModel(PluginFeatureInfo pluginFeatureInfo,
            bool showShield,
            ICoreService coreService,
            IWindowService windowService,
            INotificationService notificationService,
            IPluginManagementService pluginManagementService)
        {
            _coreService = coreService;
            _windowService = windowService;
            _notificationService = notificationService;
            _pluginManagementService = pluginManagementService;

            FeatureInfo = pluginFeatureInfo;
            ShowShield = FeatureInfo.Plugin.Info.RequiresAdmin && showShield;

            _pluginManagementService.PluginFeatureEnabling += OnFeatureEnabling;
            _pluginManagementService.PluginFeatureEnabled += OnFeatureEnableStopped;
            _pluginManagementService.PluginFeatureEnableFailed += OnFeatureEnableStopped;

            FeatureInfo.Plugin.Enabled += PluginOnToggled;
            FeatureInfo.Plugin.Disabled += PluginOnToggled;
        }

        public PluginFeatureInfo FeatureInfo { get; }
        public Exception? LoadException => FeatureInfo.LoadException;

        public bool ShowShield { get; }

        public bool Enabling
        {
            get => _enabling;
            set => this.RaiseAndSetIfChanged(ref _enabling, value);
        }

        public bool IsEnabled
        {
            get => FeatureInfo.Instance != null && FeatureInfo.Instance.IsEnabled;
            set => Task.Run(() => UpdateEnabled(value));
        }

        public bool CanToggleEnabled => FeatureInfo.Plugin.IsEnabled && !FeatureInfo.AlwaysEnabled;
        public bool CanInstallPrerequisites => FeatureInfo.Prerequisites.Any();
        public bool CanRemovePrerequisites => FeatureInfo.Prerequisites.Any(p => p.UninstallActions.Any());
        public bool IsPopupEnabled => CanInstallPrerequisites || CanRemovePrerequisites;

        public void ShowLogsFolder()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Path.Combine(Constants.DataFolder, "Logs"));
            }
            catch (Exception e)
            {
                _windowService.ShowExceptionDialog("Welp, we couldn\'t open the logs folder for you", e);
            }
        }

        public void ViewLoadException()
        {
            if (LoadException != null) 
                _windowService.ShowExceptionDialog("Feature failed to enable", LoadException);
        }

        public async Task InstallPrerequisites()
        {
            if (FeatureInfo.Prerequisites.Any()) 
                await PluginPrerequisitesInstallDialogViewModel.Show(_dialogService, new List<IPrerequisitesSubject> {FeatureInfo});
        }

        public async Task RemovePrerequisites()
        {
            if (FeatureInfo.Prerequisites.Any(p => p.UninstallActions.Any()))
            {
                await PluginPrerequisitesUninstallDialogViewModel.Show(_dialogService, new List<IPrerequisitesSubject> {FeatureInfo});
                this.RaisePropertyChanged(nameof(IsEnabled));
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pluginManagementService.PluginFeatureEnabling -= OnFeatureEnabling;
                _pluginManagementService.PluginFeatureEnabled -= OnFeatureEnableStopped;
                _pluginManagementService.PluginFeatureEnableFailed -= OnFeatureEnableStopped;

                FeatureInfo.Plugin.Enabled -= PluginOnToggled;
                FeatureInfo.Plugin.Disabled -= PluginOnToggled;
            }

            base.Dispose(disposing);
        }

        private async Task UpdateEnabled(bool enable)
        {
            if (IsEnabled == enable)
            {
                this.RaisePropertyChanged(nameof(IsEnabled));
                return;
            }

            if (FeatureInfo.Instance == null)
            {
                this.RaisePropertyChanged(nameof(IsEnabled));
                _notificationService.CreateNotification()
                    .WithMessage($"Feature '{FeatureInfo.Name}' is in a broken state and cannot enable.")
                    .HavingButton(b => b.WithText("View logs").WithAction(ShowLogsFolder))
                    .WithSeverity(NotificationSeverity.Error)
                    .Show();
                return;
            }

            if (enable)
            {
                Enabling = true;

                try
                {
                    if (FeatureInfo.Plugin.Info.RequiresAdmin && !_coreService.IsElevated)
                    {
                        bool confirmed = await _dialogService.ShowConfirmDialog("Enable feature", "The plugin of this feature requires admin rights, are you sure you want to enable it?");
                        if (!confirmed)
                        {
                            this.RaisePropertyChanged(nameof(IsEnabled));
                            return;
                        }
                    }

                    // Check if all prerequisites are met async
                    if (!FeatureInfo.ArePrerequisitesMet())
                    {
                        await PluginPrerequisitesInstallDialogViewModel.Show(_dialogService, new List<IPrerequisitesSubject> {FeatureInfo});
                        if (!FeatureInfo.ArePrerequisitesMet())
                        {
                            this.RaisePropertyChanged(nameof(IsEnabled));
                            return;
                        }
                    }

                    await Task.Run(() => _pluginManagementService.EnablePluginFeature(FeatureInfo.Instance!, true));
                }
                catch (Exception e)
                {
                    _notificationService.CreateNotification()
                        .WithMessage($"Failed to enable '{FeatureInfo.Name}'.\r\n{e.Message}")
                        .HavingButton(b => b.WithText("View logs").WithAction(ShowLogsFolder))
                        .WithSeverity(NotificationSeverity.Error)
                        .Show();
                }
                finally
                {
                    Enabling = false;
                }
            }
            else
            {
                _pluginManagementService.DisablePluginFeature(FeatureInfo.Instance, true);
                this.RaisePropertyChanged(nameof(IsEnabled));
            }
        }

        private void OnFeatureEnabling(object? sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature != FeatureInfo.Instance)
            {
                return;
            }

            Enabling = true;
        }

        private void OnFeatureEnableStopped(object? sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature != FeatureInfo.Instance)
            {
                return;
            }

            Enabling = false;

            this.RaisePropertyChanged(nameof(IsEnabled));
            this.RaisePropertyChanged(nameof(LoadException));
        }

        private void PluginOnToggled(object? sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(CanToggleEnabled));
        }
    }
}