using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Profiles;

namespace Artemis.InjectionFactories
{
    public interface IProfileEditorVmFactory
    {
        ProfileEditorViewModel CreateProfileEditorVm(MainManager mainManager, EffectModel gameModel, string lastProfile);
    }
}