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
        private readonly IDialogService _dialogService;
        private readonly IPluginManagementService _pluginManagementService;
        private bool _enabling;
        private readonly IMessageService _messageService;

        public PluginFeatureViewModel(PluginFeature feature,
            IDialogService dialogService,
            IPluginManagementService pluginManagementService,
            IMessageService messageService)
        {
            _dialogService = dialogService;
            _pluginManagementService = pluginManagementService;
            _messageService = messageService;

            Feature = feature;
        }

        public PluginFeature Feature { get; }
        public Exception LoadException => Feature.LoadException;

        public bool Enabling
        {
            get => _enabling;
            set => SetAndNotify(ref _enabling, value);
        }

        public bool IsEnabled
        {
            get => Feature.IsEnabled;
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

            _dialogService.ShowExceptionDialog("Feature failed to enable", Feature.LoadException);
        }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            _pluginManagementService.PluginFeatureEnabling += OnFeatureEnabling;
            _pluginManagementService.PluginFeatureEnabled += OnFeatureEnableStopped;
            _pluginManagementService.PluginFeatureEnableFailed += OnFeatureEnableStopped;
        }

        protected override void OnClose()
        {
            base.OnClose();
            _pluginManagementService.PluginFeatureEnabling -= OnFeatureEnabling;
            _pluginManagementService.PluginFeatureEnabled -= OnFeatureEnableStopped;
            _pluginManagementService.PluginFeatureEnableFailed -= OnFeatureEnableStopped;
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

                try
                {
                    await Task.Run(() => _pluginManagementService.EnablePluginFeature(Feature, true));
                }
                catch (Exception e)
                {
                    _messageService.ShowMessage($"Failed to enable {Feature.Info.Name}\r\n{e.Message}", "VIEW LOGS", ShowLogsFolder);
                }
                finally
                {
                    Enabling = false;
                }
            }
            else
            {
                _pluginManagementService.DisablePluginFeature(Feature, true);
                NotifyOfPropertyChange(nameof(IsEnabled));
            }
        }

        #region Event handlers

        private void OnFeatureEnabling(object sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature != Feature) return;
            Enabling = true;
        }

        private void OnFeatureEnableStopped(object sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature != Feature) return;
            Enabling = false;

            NotifyOfPropertyChange(nameof(IsEnabled));
            NotifyOfPropertyChange(nameof(LoadException));
        }

        #endregion
    }
}