using Artemis.Core.Plugins.Interfaces;

namespace Artemis.Core.Events
{
    public class ModuleEventArgs : System.EventArgs
    {
        public IPlugin Plugin { get; set; }
    }
}