using System.ComponentModel;
using System.Timers;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Services;
using Artemis.Settings;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;
using Ninject;
using Ninject.Extensions.Logging;
using Ninject.Parameters;

namespace Artemis.ViewModels.Abstract
{
    public abstract class GameViewModel : Screen
    {
        private GameSettings _gameSettings;

        protected GameViewModel(MainManager mainManager, GameModel gameModel, IKernel kernel)
        {
            MainManager = mainManager;
            GameModel = gameModel;
            GameSettings = gameModel.Settings;

            IParameter[] args =
            {
                new ConstructorArgument("mainManager", mainManager),
                new ConstructorArgument("gameModel", gameModel),
                new ConstructorArgument("lastProfile", GameSettings.LastProfile)
            };
            ProfileEditor = kernel.Get<ProfileEditorViewModel>(args);
            ProfileEditor.PropertyChanged += ProfileUpdater;

            GameModel.Profile = ProfileEditor.SelectedProfile;
        }

        [Inject]
        public ILogger Logger { get; set; }

        [Inject]
        public ProfilePreviewModel ProfilePreviewModel { get; set; }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public ProfileEditorViewModel ProfileEditor { get; set; }

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

        public bool GameEnabled => MainManager.EffectManager.ActiveEffect == GameModel;

        public void ToggleEffect()
        {
            GameModel.Enabled = GameSettings.Enabled;
        }

        public void SaveSettings()
        {
            GameSettings?.Save();
            ProfileEditor.SaveSelectedProfile();

            if (!GameEnabled)
                return;
            
            // Restart the game if it's currently running to apply settings.
            MainManager.EffectManager.ChangeEffect(GameModel, MainManager.LoopManager);
        }

        public async void ResetSettings()
        {
            var resetConfirm =
                await DialogService.ShowQuestionMessageBox("Reset effect settings",
                    "Are you sure you wish to reset this effect's settings? \nAny changes you made will be lost.");

            if (!resetConfirm.Value)
                return;

            GameSettings.Reset(true);
            NotifyOfPropertyChange(() => GameSettings);

            SaveSettings();
        }

        private void ProfileUpdater(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName != "SelectedProfile") && IsActive)
                return;

            GameModel.Profile = ProfileEditor.SelectedProfile;
            ProfilePreviewModel.Profile = ProfileEditor.SelectedProfile;

            // Only update the last selected profile if it the editor was active and the new profile isn't null
            if ((e.PropertyName != "SelectedProfile") || !ProfileEditor.ProfileViewModel.Activated ||
                (ProfileEditor.ProfileViewModel.SelectedProfile == null))
                return;

            GameSettings.LastProfile = ProfileEditor.ProfileViewModel.SelectedProfile.Name;
            GameSettings.Save();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ProfileEditor.Activate();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            ProfileEditor.Deactivate();
        }
    }

    public delegate void OnLayersUpdatedCallback(object sender);
}