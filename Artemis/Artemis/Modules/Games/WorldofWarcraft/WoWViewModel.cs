using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Games.WorldofWarcraft
{
    public sealed class WoWViewModel : GameViewModel
    {
        public WoWViewModel(MainManager main, IEventAggregator events, IProfileEditorVmFactory pFactory)
            : base(main, new WoWModel(main, new WoWSettings()), events, pFactory)
        {
            DisplayName = "World of Warcraft";
            MainManager.EffectManager.EffectModels.Add(GameModel);
        }
    }
}