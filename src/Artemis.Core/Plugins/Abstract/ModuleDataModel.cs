using Artemis.Core.Plugins.Interfaces;
using Stylet;

namespace Artemis.Core.Plugins.Abstract
{
    public abstract class ModuleDataModel
    {
        protected ModuleDataModel(IModule module)
        {
            Module = module;
        }

        public IModule Module { get; }
    }
}