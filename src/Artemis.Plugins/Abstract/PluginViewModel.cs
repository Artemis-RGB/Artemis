using Artemis.Plugins.Interfaces;
using Artemis.Plugins.Models;
using Stylet;

namespace Artemis.Plugins.Abstract
{
    public abstract class PluginViewModel : Screen, IPluginViewModel
    {
        public PluginInfo PluginInfo { get; set; }
    }
}