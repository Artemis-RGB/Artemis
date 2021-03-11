using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginFeatureViewModel : Screen
    {
        private readonly ICoreService _coreService;
        private readonly IDialogService _dialogService;
        private readonly IPluginManagementService _pluginManagementService;
        private bool _enabling;
        private readonly IMessageService _messageService;

        public PluginFeatureViewModel(PluginFeatureInfo pluginFeatureInfo, 
            bool showShield,
            ICoreService coreService,
            IDialogService dialogService,
            IPluginManagementService pluginManagementService,
            IMessageService messageService)
        {
            _coreService = coreService;
            _dialogService = dialogService;
            _pluginManagementService = pluginManagementService;
            _messageService = messageService;

            FeatureInfo = pluginFeatureInfo;
            ShowShield = FeatureInfo.Plugin.Info.RequiresAdmin && showShield;
        }

        public PluginFeatureInfo FeatureInfo { get; }
        public Exception LoadException => FeatureInfo.Instance?.LoadException;

        public bool ShowShield { get; }

        public bool Enabling
        {
            get => _enabling;
            set => SetAndNotify(ref _enabling, value);
        }

        public bool IsEnabled
        {
            get => FeatureInfo.Instance != null && FeatureInfo.Instance.IsEnabled;
            set => Task.Run(() => UpdateEnabled(value));
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

        public void ViewLoadException()
        {
            if (LoadException == null)
                return;

            _dialogService.ShowExceptionDialog("Feature failed to enable", LoadException);
        }

        protected override void OnInitialActivate()
        {
            _pluginManagementService.PluginFeatureEnabling += OnFeatureEnabling;
            _pluginManagementService.PluginFeatureEnabled += OnFeatureEnableStopped;
            _pluginManagementService.PluginFeatureEnableFailed += OnFeatureEnableStopped;

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _pluginManagementService.PluginFeatureEnabling -= OnFeatureEnabling;
            _pluginManagementService.PluginFeatureEnabled -= OnFeatureEnableStopped;
            _pluginManagementService.PluginFeatureEnableFailed -= OnFeatureEnableStopped;

            base.OnClose();
        }

        private async Task UpdateEnabled(bool enable)
        {
            if (IsEnabled == enable || FeatureInfo.Instance == null)
            {
                NotifyOfPropertyChange(nameof(IsEnabled));
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
                            NotifyOfPropertyChange(nameof(IsEnabled));
                            return;
                        }
                    }

                    await Task.Run(() => _pluginManagementService.EnablePluginFeature(FeatureInfo.Instance, true));
                }
                catch (Exception e)
                {
                    _messageService.ShowMessage($"Failed to enable {FeatureInfo.Name}\r\n{e.Message}", "VIEW LOGS", ShowLogsFolder);
                }
                finally
                {
                    Enabling = false;
                }
            }
            else
            {
                _pluginManagementService.DisablePluginFeature(FeatureInfo.Instance, true);
                NotifyOfPropertyChange(nameof(IsEnabled));
            }
        }

        #region Event handlers

        private void OnFeatureEnabling(object sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature != FeatureInfo.Instance) return;
            Enabling = true;
        }

        private void OnFeatureEnableStopped(object sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature != FeatureInfo.Instance) return;
            Enabling = false;

            NotifyOfPropertyChange(nameof(IsEnabled));
            NotifyOfPropertyChange(nameof(LoadException));
        }

        #endregion
    }
}