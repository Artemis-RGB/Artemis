using Artemis.Core.Plugins.Abstract;
using Artemis.UI.Screens.Module;

namespace Artemis.UI.Ninject.Factories
{
    public interface IModuleViewModelFactory
    {
        ModuleRootViewModel CreateModuleViewModel(Module module);
    }
}