using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsTabViewModel : Screen
    {
        private readonly IPluginService _pluginService;
        private readonly ISettingsVmFactory _settingsVmFactory;
        private BindableCollection<PluginSettingsViewModel> _plugins;

        public PluginSettingsTabViewModel(IPluginService pluginService, ISettingsVmFactory settingsVmFactory)
        {
            DisplayName = "PLUGINS";

            _pluginService = pluginService;
            _settingsVmFactory = settingsVmFactory;

            Plugins = new BindableCollection<PluginSettingsViewModel>();
        }

        public BindableCollection<PluginSettingsViewModel> Plugins
        {
            get => _plugins;
            set => SetAndNotify(ref _plugins, value);
        }

        protected override void OnActivate()
        {
            // Take it off the UI thread to avoid freezing on tab change
            Task.Run(() =>
            {
                Plugins.Clear();
                var instances = _pluginService.GetAllPluginInfo().Select(p => _settingsVmFactory.CreatePluginSettingsViewModel(p.Instance)).ToList();
                foreach (var pluginSettingsViewModel in instances)
                    Plugins.Add(pluginSettingsViewModel);
            });
        }
    }
}