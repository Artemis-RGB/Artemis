using Artemis.Managers;
using Artemis.Models;
using Artemis.Services;
using Caliburn.Micro;
using Ninject;

namespace Artemis.ViewModels.Abstract
{
    public abstract class EffectViewModel : Screen
    {
        protected readonly EffectModel EffectModel;
        private EffectSettings _effectSettings;
        private bool _showDisabledPopup;
        protected MainManager MainManager;

        protected EffectViewModel(MainManager mainManager, EffectModel effectModel)
        {
            MainManager = mainManager;
            EffectModel = effectModel;
        }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public EffectSettings EffectSettings
        {
            get { return _effectSettings; }
            set
            {
                if (Equals(value, _effectSettings)) return;
                _effectSettings = value;
                NotifyOfPropertyChange(() => EffectSettings);
            }
        }

        public bool EffectEnabled => MainManager.EffectManager.ActiveEffect == EffectModel;

        public bool ShowDisabledPopup
        {
            get { return _showDisabledPopup; }
            set
            {
                if (value == _showDisabledPopup) return;
                _showDisabledPopup = value;
                NotifyOfPropertyChange(() => ShowDisabledPopup);
            }
        }

        public void ToggleEffect()
        {
            if (!MainManager.ProgramEnabled)
            {
                NotifyOfPropertyChange(() => EffectEnabled);
                ShowDisabledPopup = true;
                return;
            }

            if (EffectEnabled)
                MainManager.EffectManager.ClearEffect();
            else
                MainManager.EffectManager.ChangeEffect(EffectModel, MainManager.LoopManager);
        }

        public void SaveSettings()
        {
            EffectSettings?.Save();
            if (!EffectEnabled)
                return;

            // Restart the effect if it's currently running to apply settings.
            MainManager.EffectManager.ChangeEffect(EffectModel);
        }

        public async void ResetSettings()
        {
            var resetConfirm = await
                DialogService.ShowQuestionMessageBox("Reset effect settings",
                    "Are you sure you wish to reset this effect's settings? \nAny changes you made will be lost.");

            if (!resetConfirm.Value)
                return;

            EffectSettings.ToDefault();
            NotifyOfPropertyChange(() => EffectSettings);

            SaveSettings();
        }
    }
}