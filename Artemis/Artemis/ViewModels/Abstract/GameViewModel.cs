﻿using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Services;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;
using Ninject;
using Ninject.Extensions.Logging;

namespace Artemis.ViewModels.Abstract
{
    public abstract class GameViewModel : Screen
    {
        private GameSettings _gameSettings;

        protected GameViewModel(MainManager mainManager, GameModel gameModel, IEventAggregator events,
            IProfileEditorViewModelFactory pFactory)
        {
            MainManager = mainManager;
            GameModel = gameModel;
            Events = events;
            PFactory = pFactory;
            GameSettings = gameModel.Settings;

            ProfileEditor = PFactory.CreateProfileEditorViewModel(Events, mainManager, gameModel);
            GameModel.Profile = ProfileEditor.SelectedProfile;
            ProfileEditor.PropertyChanged += ProfileUpdater;
            Events.Subscribe(this);
        }

        [Inject]
        public ILogger Logger { get; set; }

        [Inject]
        public ProfilePreviewModel ProfilePreviewModel { get; set; }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public IEventAggregator Events { get; set; }
        public IProfileEditorViewModelFactory PFactory { get; set; }

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

            GameSettings.ToDefault();
            NotifyOfPropertyChange(() => GameSettings);

            SaveSettings();
        }
        
        private void ProfileUpdater(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedProfile" && IsActive)
                return;

            GameModel.Profile = ProfileEditor.SelectedProfile;
            ProfilePreviewModel.SelectedProfile = ProfileEditor.SelectedProfile;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ProfileEditor.ProfileViewModel.Activate();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            ProfileEditor.ProfileViewModel.Deactivate();
        }
    }

    public delegate void OnLayersUpdatedCallback(object sender);
}