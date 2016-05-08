using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.TheDivision
{
    public sealed class TheDivisionViewModel : GameViewModel<TheDivisionDataModel>
    {
        public TheDivisionViewModel(MainManager main)
            : base(main, new TheDivisionModel(main, new TheDivisionSettings()))
        {
            DisplayName = "The Division";
            MainManager.EffectManager.EffectModels.Add(GameModel);
        }
    }
}