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
        private readonly IPluginService _pluginService;
        private readonly ISettingsVmFactory _settingsVmFactory;
        private BindableCollection<PluginSettingsViewModel> _plugins;

        public PluginSettingsTabViewModel(IPluginService pluginService, ISettingsVmFactory settingsVmFactory)
        {
            DisplayName = "PLUGINS";

            _pluginService = pluginService;
            _settingsVmFactory = settingsVmFactory;
        }

        protected override void OnActivate()
        {
            // Take it off the UI thread to avoid freezing on tab change
            Task.Run(async () =>
            {
                Items.Clear();
                await Task.Delay(200);

                List<PluginSettingsViewModel> instances = _pluginService.GetAllPluginInfo().Select(p => _settingsVmFactory.CreatePluginSettingsViewModel(p.Instance)).ToList();
                foreach (PluginSettingsViewModel pluginSettingsViewModel in instances) 
                    Items.Add(pluginSettingsViewModel);
            });

            base.OnActivate();
        }
    }
}