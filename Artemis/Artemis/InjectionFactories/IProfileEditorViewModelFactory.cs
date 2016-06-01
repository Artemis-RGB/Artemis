using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;

namespace Artemis.InjectionFactories
{
    public interface IProfileEditorViewModelFactory
    {
        ProfileEditorViewModel CreateProfileEditorViewModel(IEventAggregator events, MainManager mainManager, GameModel gameModel, string lastProfile);
    }
}