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

        public PluginSettingsTabViewModel(IPluginManagementService pluginManagementService, ISettingsVmFactory settingsVmFactory)
        {
            DisplayName = "PLUGINS";

            _pluginManagementService = pluginManagementService;
            _settingsVmFactory = settingsVmFactory;
        }

        protected override void OnActivate()
        {
            // Take it off the UI thread to avoid freezing on tab change
            Task.Run(async () =>
            {
                Items.Clear();
                await Task.Delay(200);

                List<PluginSettingsViewModel> instances = _pluginManagementService.GetAllPlugins().Select(p => _settingsVmFactory.CreatePluginSettingsViewModel(p)).ToList();
                foreach (PluginSettingsViewModel pluginSettingsViewModel in instances.OrderBy(i => i.Plugin.Info.Name)) 
                    Items.Add(pluginSettingsViewModel);
            });

            base.OnActivate();
        }
    }
}