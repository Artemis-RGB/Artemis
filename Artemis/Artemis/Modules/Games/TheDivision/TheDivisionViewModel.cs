using Artemis.Managers;
using Artemis.ViewModels;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.TheDivision
{
    public class TheDivisionViewModel : GameViewModel<TheDivisionDataModel>
    {
        public TheDivisionViewModel(MainManager mainManager) : base(mainManager, new TheDivisionModel(mainManager, new TheDivisionSettings()))
        {
            MainManager.EffectManager.EffectModels.Add(GameModel);
        }

        public static string Name => "The Division";
    }
}