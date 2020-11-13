using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsTabViewModel : Conductor<PluginSettingsViewModel>.Collection.AllActive
    {
        private readonly IPluginManagementService _pluginManagementService;
        private readonly ISettingsVmFactory _settingsVmFactory;
        private string _searchPluginInput;
        private List<PluginSettingsViewModel> _instances;

        public PluginSettingsTabViewModel(IPluginManagementService pluginManagementService, ISettingsVmFactory settingsVmFactory)
        {
            DisplayName = "PLUGINS";

            _pluginManagementService = pluginManagementService;
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

            Items.Clear();

            if (string.IsNullOrWhiteSpace(SearchPluginInput))
                Items.AddRange(_instances);
            else
                Items.AddRange(_instances.Where(i => i.Plugin.Info.Name.Contains(SearchPluginInput, StringComparison.OrdinalIgnoreCase) ||
                                                     i.Plugin.Info.Description.Contains(SearchPluginInput, StringComparison.OrdinalIgnoreCase)));
        }

        protected override void OnActivate()
        {
            // Take it off the UI thread to avoid freezing on tab change
            Task.Run(async () =>
            {
                await Task.Delay(200);
                _instances = _pluginManagementService.GetAllPlugins()
                    .Select(p => _settingsVmFactory.CreatePluginSettingsViewModel(p))
                    .OrderBy(i => i.Plugin.Info.Name)
                    .ToList();

                UpdatePluginSearch();
            });

            base.OnActivate();
        }
    }
}