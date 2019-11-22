using Artemis.Core.Plugins.Abstract;
using Artemis.UI.Screens.Module;

namespace Artemis.UI.Ninject.Factories
{
    public interface IModuleViewModelFactory : IArtemisUIFactory
    {
        ModuleRootViewModel Create(Module module);
    }
}