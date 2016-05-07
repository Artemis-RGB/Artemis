using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.TheDivision
{
    public sealed class TheDivisionViewModel : GameViewModel<TheDivisionDataModel>
    {
        public TheDivisionViewModel(MainManager mainManager, EffectManager effectManager,
            KeyboardManager keyboardManager)
            : base(
                mainManager, effectManager,
                new TheDivisionModel(mainManager, keyboardManager, new TheDivisionSettings()))
        {
            DisplayName = "The Division";
            EffectManager.EffectModels.Add(GameModel);
        }
    }
}