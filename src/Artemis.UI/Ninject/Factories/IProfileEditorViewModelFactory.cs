using Artemis.Core.Plugins.Abstract;
using Artemis.UI.ViewModels.Controls.ProfileEditor;

namespace Artemis.UI.Ninject.Factories
{
    public interface IProfileEditorViewModelFactory
    {
        ProfileEditorViewModel CreateModuleViewModel(Module module);
    }
}