using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.DeviceProviders;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using Humanizer;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginFeatureViewModel : Screen
    {
        private readonly IDialogService _dialogService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private bool _enabling;
        private bool _isEnabled;

        public PluginFeatureViewModel(PluginFeature feature,
            IDialogService dialogService,
            IPluginManagementService pluginManagementService,
            ISnackbarMessageQueue snackbarMessageQueue)
        {
            _dialogService = dialogService;
            _pluginManagementService = pluginManagementService;
            _snackbarMessageQueue = snackbarMessageQueue;

            Feature = feature;
            Icon = GetIconKind();

            IsEnabled = Feature.IsEnabled;
        }

        public PluginFeature Feature { get; }
        public PackIconKind Icon { get; }

        public string Name => Feature.GetType().Name.Humanize();

        public Exception LoadException => Feature.LoadException;

        public bool Enabling
        {
            get => _enabling;
            set => SetAndNotify(ref _enabling, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetAndNotify(ref _isEnabled, value);
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
                    await Task.Run(() => _pluginManagementService.EnablePluginFeature(Feature));
                }
                catch (Exception e)
                {
                    _snackbarMessageQueue.Enqueue($"Failed to enable {Name}\r\n{e.Message}", "VIEW LOGS", ShowLogsFolder);
                }
                finally
                {
                    Enabling = false;
                }
            }
            else
            {
                _pluginManagementService.DisablePluginFeature(Feature);
            }
        }

        private PackIconKind GetIconKind()
        {
            switch (Feature)
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

        #region Event handlers

        private void OnFeatureEnabling(object? sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature != Feature) return;
            Enabling = true;
        }

        private void OnFeatureEnableStopped(object? sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature != Feature) return;
            Enabling = false;
            IsEnabled = e.PluginFeature.IsEnabled;

            NotifyOfPropertyChange(nameof(LoadException));
        }

        #endregion
    }
}