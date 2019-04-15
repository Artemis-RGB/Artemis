using Stylet;

namespace Artemis.Core.Plugins.Abstract
{
    public abstract class ModuleViewModel : Screen
    {
        protected ModuleViewModel(Module module)
        {
            Module = module;
        }

        public Module Module { get; }
    }
}