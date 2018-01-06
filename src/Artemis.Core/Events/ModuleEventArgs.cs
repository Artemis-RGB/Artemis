using Artemis.Core.Modules.Interfaces;

namespace Artemis.Core.Events
{
    public class ModuleEventArgs : System.EventArgs
    {
        public IModule Module { get; set; }
    }
}