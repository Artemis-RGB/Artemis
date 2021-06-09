using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Ookii.Dialogs.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsTabViewModel : Conductor<PluginSettingsViewModel>.Collection.AllActive
    {
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IMessageService _messageService;
        private readonly ISettingsVmFactory _settingsVmFactory;
        private string _searchPluginInput;
        private List<PluginSettingsViewModel> _instances;

        public PluginSettingsTabViewModel(IPluginManagementService pluginManagementService, IMessageService messageService, ISettingsVmFactory settingsVmFactory)
        {
            DisplayName = "PLUGINS";

            _pluginManagementService = pluginManagementService;
            _messageService = messageService;
            _settingsVmFactory = settingsVmFactory;
        }

        public string SearchPluginInput
        {
            get => _searchPluginInput;
            set
            {
                if (!SetAndNotify(ref _searchPluginInput, value)) return;
                UpdatePluginSearch();
            }
        }

        private void UpdatePluginSearch()
        {
            if (_instances == null)
                return;

            List<PluginSettingsViewModel> instances = _instances;
            string search = SearchPluginInput?.ToLower();
            if (!string.IsNullOrWhiteSpace(search))
                instances = instances.Where(i => i.Plugin.Info.Name.ToLower().Contains(search) ||
                                                 i.Plugin.Info.Description != null && i.Plugin.Info.Description.ToLower().Contains(search)).ToList();

            foreach (PluginSettingsViewModel pluginSettingsViewModel in instances)
            {
                if (!Items.Contains(pluginSettingsViewModel))
                    Items.Add(pluginSettingsViewModel);
            }

            foreach (PluginSettingsViewModel pluginSettingsViewModel in Items.ToList())
            {
                if (!instances.Contains(pluginSettingsViewModel))
                    Items.Remove(pluginSettingsViewModel);
            }

            ((BindableCollection<PluginSettingsViewModel>) Items).Sort(i => i.Plugin.Info.Name);
        }

        protected override void OnInitialActivate()
        {
            // Take it off the UI thread to avoid freezing on tab change
            Task.Run(async () =>
            {
                await Task.Delay(200);
                GetPluginInstances();
            });

            base.OnInitialActivate();
        }

        public async Task ImportPlugin()
        {
            VistaOpenFileDialog dialog = new() {Filter = "ZIP files (*.zip)|*.zip", Title = "Import Artemis plugin"};
            bool? result = dialog.ShowDialog();
            if (result != true)
                return;

            // Take the actual import off of the UI thread
            await Task.Run(() =>
            {
                Plugin plugin = _pluginManagementService.ImportPlugin(dialog.FileName);

                GetPluginInstances();
                SearchPluginInput = plugin.Info.Name;

                // Enable it via the VM to enable the prerequisite dialog
                PluginSettingsViewModel pluginViewModel = Items.FirstOrDefault(i => i.Plugin == plugin);
                if (pluginViewModel is {IsEnabled: false})
                    pluginViewModel.IsEnabled = true;

                _messageService.ShowMessage($"Imported plugin: {plugin.Info.Name}");
            });
        }

        public void GetPluginInstances()
        {
            _instances = _pluginManagementService.GetAllPlugins()
                .Select(p => _settingsVmFactory.CreatePluginSettingsViewModel(p))
                .OrderBy(i => i.Plugin.Info.Name)
                .ToList();

            UpdatePluginSearch();
        }
    }
}