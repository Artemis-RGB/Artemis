using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Plugins.Models;
using Stylet;

namespace Artemis.Core.Plugins.Abstract
{
    public abstract class ModuleViewModel : Screen, IModuleViewModel
    {
        public PluginInfo PluginInfo { get; set; }
    }
}