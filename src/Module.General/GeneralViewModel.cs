using Artemis.Plugins.Interfaces;
using Artemis.Plugins.Models;
using Stylet;

namespace Module.General
{
    public class GeneralViewModel : Screen, IModuleViewModel
    {
        public PluginInfo PluginInfo { get; set; }
    }
}