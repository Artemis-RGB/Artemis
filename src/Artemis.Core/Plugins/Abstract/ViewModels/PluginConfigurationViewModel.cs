using Stylet;

namespace Artemis.Core.Plugins.Abstract.ViewModels
{
    public abstract class PluginConfigurationViewModel : Screen
    {
        protected PluginConfigurationViewModel(Plugin plugin)
        {
            Plugin = plugin;
        }

        public Plugin Plugin { get; set; }
    }
}