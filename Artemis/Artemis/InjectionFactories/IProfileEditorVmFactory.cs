using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;

namespace Artemis.InjectionFactories
{
    public interface IProfileEditorVmFactory
    {
        ProfileEditorViewModel CreateProfileEditorVm(IEventAggregator events, MainManager mainManager,
            EffectModel gameModel, string lastProfile);
    }
}