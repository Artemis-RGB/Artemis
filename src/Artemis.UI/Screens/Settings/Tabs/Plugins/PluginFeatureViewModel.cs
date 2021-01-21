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
        private IMessageService _messageService;
        private bool _enabling;
        
        public PluginFeatureViewModel(PluginFeature feature,
            IDialogService dialogService,
            IPluginManagementService pluginManagementService,
            IMessageService messageService)
        {
            _dialogService = dialogService;
            _pluginManagementService = pluginManagementService;
            _messageService = messageService;

            Feature = feature;
            Icon = GetIconKind();
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
                    _messageService.ShowMessage($"Failed to enable {Name}\r\n{e.Message}", "VIEW LOGS", ShowLogsFolder);
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

        private PackIconKind GetIconKind()
        {
            return Feature switch
            {
                BaseDataModelExpansion => PackIconKind.TableAdd,
                DeviceProvider => PackIconKind.Devices,
                ProfileModule => PackIconKind.VectorRectangle,
                Module => PackIconKind.GearBox,
                LayerBrushProvider => PackIconKind.Brush,
                LayerEffectProvider => PackIconKind.AutoAwesome,
                _ => PackIconKind.Plugin
            };
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