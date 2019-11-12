using Stylet;

namespace Artemis.Core.Plugins.Abstract
{
    public abstract class ModuleViewModel : Screen
    {
        protected ModuleViewModel(Module module, string name)
        {
            Module = module;
            Name = name;
        }

        public string Name { get; }
        public Module Module { get; }
    }
}