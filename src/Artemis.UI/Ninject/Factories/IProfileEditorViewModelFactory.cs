using Artemis.Core.Plugins.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor;

namespace Artemis.UI.Ninject.Factories
{
    public interface IProfileEditorViewModelFactory : IArtemisUIFactory
    {
        ProfileEditorViewModel Create(ProfileModule module);
    }
}