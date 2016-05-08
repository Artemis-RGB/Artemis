using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Games.TheDivision
{
    public sealed class TheDivisionViewModel : GameViewModel
    {
        public TheDivisionViewModel(MainManager main, IEventAggregator events, IProfileEditorViewModelFactory pFactory)
            : base(main, new TheDivisionModel(main, new TheDivisionSettings()), events, pFactory)
        {
            DisplayName = "The Division";
            MainManager.EffectManager.EffectModels.Add(GameModel);
        }
    }
}