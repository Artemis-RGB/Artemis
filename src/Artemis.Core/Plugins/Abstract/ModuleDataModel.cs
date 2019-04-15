namespace Artemis.Core.Plugins.Abstract
{
    public abstract class ModuleDataModel
    {
        protected ModuleDataModel(Module module)
        {
            Module = module;
        }

        public Module Module { get; }
    }
}