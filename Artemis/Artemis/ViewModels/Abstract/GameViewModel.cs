using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Modules.Effects.ProfilePreview;
using Caliburn.Micro;

namespace Artemis.ViewModels.Abstract
{
    public abstract class GameViewModel<T> : Screen
    {
        private bool _doActivate;

        private bool _editorShown;
        private GameSettings _gameSettings;
        private EffectModel _lastEffect;
        protected EffectManager EffectManager;

        protected GameViewModel(MainManager mainManager, EffectManager effectManager, GameModel gameModel)
        {
            MainManager = mainManager;
            EffectManager = effectManager;
            GameModel = gameModel;
            GameSettings = gameModel.Settings;

            //ProfileEditor = new ProfileEditorViewModel<T>(MainManager, GameModel);
            GameModel.Profile = ProfileEditor.SelectedProfile;
            ProfileEditor.PropertyChanged += ProfileUpdater;
        }

        public ProfileEditorViewModel<T> ProfileEditor { get; set; }

        public GameModel GameModel { get; set; }
        public MainManager MainManager { get; set; }

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

        public bool GameEnabled => EffectManager.ActiveEffect == GameModel;

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
            EffectManager.ChangeEffect(GameModel, true);
        }

        public async void ResetSettings()
        {
            var resetConfirm =
                await
                    MainManager.DialogService.ShowQuestionMessageBox("Reset effect settings",
                        "Are you sure you wish to reset this effect's settings? \nAny changes you made will be lost.");

            if (!resetConfirm.Value)
                return;

            GameSettings.ToDefault();
            NotifyOfPropertyChange(() => GameSettings);

            SaveSettings();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            // OnActive is triggered at odd moments, only activate the profile 
            // preview if OnDeactivate isn't called right after it
            _doActivate = true;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                if (_doActivate)
                    SetEditorShown(true);
            });
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            _doActivate = false;
            SetEditorShown(false);
        }

        public void SetEditorShown(bool enable)
        {
            if (enable == _editorShown)
                return;

            if (enable)
            {
                // Store the current effect so it can be restored later
                if (!(EffectManager.ActiveEffect is ProfilePreviewModel))
                    _lastEffect = EffectManager.ActiveEffect;

                EffectManager.ProfilePreviewModel.SelectedProfile = ProfileEditor.SelectedProfile;
                EffectManager.ChangeEffect(EffectManager.ProfilePreviewModel);
            }
            else
            {
                if (_lastEffect != null)
                {
                    // Game models are only used if they are enabled
                    var gameModel = _lastEffect as GameModel;
                    if (gameModel != null)
                        if (!gameModel.Enabled)
                            EffectManager.GetLastEffect();
                        else
                            EffectManager.ChangeEffect(_lastEffect, true);
                }
                else
                    EffectManager.ClearEffect();
            }

            _editorShown = enable;
        }

        private void ProfileUpdater(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedProfile")
                return;

            GameModel.Profile = ProfileEditor.SelectedProfile;
            EffectManager.ProfilePreviewModel.SelectedProfile = ProfileEditor.SelectedProfile;
        }
    }

    public delegate void OnLayersUpdatedCallback(object sender);
}