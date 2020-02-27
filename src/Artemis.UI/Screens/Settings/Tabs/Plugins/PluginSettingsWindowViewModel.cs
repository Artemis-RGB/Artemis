using Artemis.Core.Plugins.Abstract.ViewModels;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsWindowViewModel : Conductor<PluginConfigurationViewModel>
    {
        public PluginSettingsWindowViewModel(PluginConfigurationViewModel configurationViewModel)
        {
            ActiveItem = configurationViewModel;
        }

        public string Title => "Plugin configuration - " + ActiveItem?.Plugin?.PluginInfo?.Name;
    }
}