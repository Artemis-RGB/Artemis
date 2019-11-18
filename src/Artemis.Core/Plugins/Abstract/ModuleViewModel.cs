using Stylet;

namespace Artemis.Core.Plugins.Abstract
{
    public abstract class ModuleViewModel : Screen
    {
        protected ModuleViewModel(Module module, string displayName)
        {
            Module = module;
            DisplayName = displayName;
        }
        
        public Module Module { get; }
    }
}