using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Plugins.Models;
using Stylet;

namespace Artemis.Plugins.Modules.General.ViewModels
{
    public class GeneralViewModel : Screen, IModuleViewModel
    {
        public PluginInfo PluginInfo { get; set; }
    }
}