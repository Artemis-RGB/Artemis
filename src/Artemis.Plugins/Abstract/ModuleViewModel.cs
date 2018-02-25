using Artemis.Plugins.Interfaces;
using Artemis.Plugins.Models;
using Stylet;

namespace Artemis.Plugins.Abstract
{
    public abstract class ModuleViewModel : Screen, IModuleViewModel
    {
        public PluginInfo PluginInfo { get; set; }
    }
}