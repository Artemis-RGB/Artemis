using Artemis.Managers;
using Artemis.Models;
using Caliburn.Micro;

namespace Artemis.ViewModels.Abstract
{
    public abstract class GameViewModel : Screen
    {
        private GameSettings _gameSettings;

        public GameModel GameModel { get; set; }
        public MainManager MainManager { get; set; }
        public event OnLayersUpdatedCallback OnLayersUpdatedCallback;
        public GameSettings GameSettings
        {
            get { return _gameSettings; }
            set
            {
                if (Equals(value, _gameSettings)) return;
                _gameSettings = value;
                NotifyOfPropertyChange(() => GameSettings);
            }
        }

        public bool GameEnabled => MainManager.EffectManager.ActiveEffect == GameModel;

        public void ToggleEffect()
        {
            GameModel.Enabled = GameSettings.Enabled;
        }

        public void SaveSettings()
        {
            GameSettings?.Save();
            if (!GameEnabled)
                return;

            // Restart the game if it's currently running to apply settings.
            MainManager.EffectManager.ChangeEffect(GameModel, true);
        }

        public async void ResetSettings()
        {
            var resetConfirm = await
                MainManager.DialogService.ShowQuestionMessageBox("Reset effect settings",
                    "Are you sure you wish to reset this effect's settings? \nAny changes you made will be lost.");

            if (!resetConfirm.Value)
                return;

            GameSettings.ToDefault();
            NotifyOfPropertyChange(() => GameSettings);

            SaveSettings();
        }
    }

    public delegate void OnLayersUpdatedCallback(object sender);
}