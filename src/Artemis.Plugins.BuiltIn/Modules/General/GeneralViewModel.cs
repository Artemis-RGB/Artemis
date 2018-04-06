using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Plugins.Models;
using Stylet;

namespace Artemis.Plugins.BuiltIn.Modules.General
{
    public class GeneralViewModel : Screen, IModuleViewModel
    {
        public PluginInfo PluginInfo { get; set; }
    }
}