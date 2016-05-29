using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Games.Overwatch
{
    public sealed class OverwatchViewModel : GameViewModel
    {
        public OverwatchViewModel(MainManager main, IEventAggregator events, IProfileEditorViewModelFactory pFactory)
            : base(main, new OverwatchModel(events, main, new OverwatchSettings()), events, pFactory)
        {
            DisplayName = "Overwatch";
            MainManager.EffectManager.EffectModels.Add(GameModel);
        }
    }
}