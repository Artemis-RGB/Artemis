using Artemis.Managers;
using Artemis.ViewModels;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.TheDivision
{
    public class TheDivisionViewModel : GameViewModel
    {
        public TheDivisionViewModel(MainManager mainManager)
        {
            MainManager = mainManager;

            // Settings are loaded from file by class
            GameSettings = new TheDivisionSettings();

            // Create effect model and add it to MainManager
            GameModel = new TheDivisionModel(mainManager, (TheDivisionSettings) GameSettings);
            MainManager.EffectManager.EffectModels.Add(GameModel);

            ProfileEditor = new ProfileEditorViewModel(GameModel, MainManager.KeyboardManager.ActiveKeyboard);
        }

        public ProfileEditorViewModel ProfileEditor { get; set; }

        public static string Name => "The Division";
    }
}