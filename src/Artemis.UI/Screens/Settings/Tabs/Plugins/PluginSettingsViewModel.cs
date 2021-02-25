﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsViewModel : Conductor<PluginFeatureViewModel>.Collection.AllActive
    {
        private readonly IDialogService _dialogService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly ISettingsVmFactory _settingsVmFactory;
        private readonly ICoreService _coreService;
        private readonly IMessageService _messageService;
        private readonly IWindowManager _windowManager;
        private bool _enabling;
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

        public void OpenSettings()
        {
            PluginConfigurationDialog configurationViewModel = (PluginConfigurationDialog) Plugin.ConfigurationDialog;
            if (configurationViewModel == null)
                return;

            try
            {
                PluginConfigurationViewModel viewModel = (PluginConfigurationViewModel) Plugin.Kernel.Get(configurationViewModel.Type);
                _windowManager.ShowDialog(new PluginSettingsWindowViewModel(viewModel, Icon));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("An exception occured while trying to show the plugin's settings window", e);
                throw;
            }
        }

        public async Task Remove()
        {
            bool confirmed = await _dialogService.ShowConfirmDialog("Delete plugin", "Are you sure you want to delete this plugin?");
            if (!confirmed)
                return;

            try
            {
                _pluginManagementService.RemovePlugin(Plugin);
                ((PluginSettingsTabViewModel) Parent).GetPluginInstances();
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("Failed to remove plugin", e);
                throw;
            }
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

        protected override void OnInitialActivate()
        {
            Plugin.FeatureAdded += PluginOnFeatureAdded;
            Plugin.FeatureRemoved += PluginOnFeatureRemoved;
            foreach (PluginFeature pluginFeature in Plugin.Features)
                Items.Add(_settingsVmFactory.CreatePluginFeatureViewModel(pluginFeature));

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            Plugin.FeatureAdded -= PluginOnFeatureAdded;
            Plugin.FeatureRemoved -= PluginOnFeatureRemoved;

            base.OnClose();
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
                        Enabling = false;
                        NotifyOfPropertyChange(nameof(IsEnabled));
                        NotifyOfPropertyChange(nameof(CanOpenSettings));
                        return;
                    }
                }

                try
                {
                    await Task.Run(() => _pluginManagementService.EnablePlugin(Plugin, true));
                }
                catch (Exception e)
                {
                    _messageService.ShowMessage($"Failed to enable plugin {Plugin.Info.Name}\r\n{e.Message}", "VIEW LOGS", ShowLogsFolder);
                }
                finally
                {
                    Enabling = false;
                }
            }
            else
            {
                _pluginManagementService.DisablePlugin(Plugin, true);
            }

            NotifyOfPropertyChange(nameof(IsEnabled));
            NotifyOfPropertyChange(nameof(CanOpenSettings));
        }

        private void PluginOnFeatureRemoved(object sender, PluginFeatureEventArgs e)
        {
            PluginFeatureViewModel viewModel = Items.FirstOrDefault(i => i.Feature == e.PluginFeature);
            if (viewModel != null)
                Items.Remove(viewModel);
        }

        private void PluginOnFeatureAdded(object sender, PluginFeatureEventArgs e)
        {
            Items.Add(_settingsVmFactory.CreatePluginFeatureViewModel(e.PluginFeature));
        }
    }
}