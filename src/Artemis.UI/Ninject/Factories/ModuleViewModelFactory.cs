using Artemis.Core.Plugins.Abstract;
using Artemis.UI.ViewModels.Screens;

namespace Artemis.UI.Ninject.Factories
{
    public interface IModuleViewModelFactory
    {
        ModuleRootViewModel CreateModuleViewModel(Module module);
    }
}