using Artemis.Managers;
using Artemis.Models;
using Caliburn.Micro;

namespace Artemis.ViewModels.Abstract
{
    public abstract class EffectViewModel : Screen
    {
        private EffectSettings _effectSettings;
        private bool _showDisabledPopup;

        public EffectModel EffectModel { get; set; }
        public MainManager MainManager { get; set; }

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
                MainManager.EffectManager.ChangeEffect(EffectModel);
        }

        public void SaveSettings()
        {
            EffectSettings?.Save();
            if (!EffectEnabled)
                return;

            // Restart the effect if it's currently running to apply settings.
            MainManager.EffectManager.ChangeEffect(EffectModel, true);
        }

        public async void ResetSettings()
        {
            var resetConfirm = await
                MainManager.DialogService.ShowQuestionMessageBox("Reset effect settings",
                    "Are you sure you wish to reset this effect's settings? \nAny changes you made will be lost.");

            if (!resetConfirm.Value)
                return;

            EffectSettings.ToDefault();
            NotifyOfPropertyChange(() => EffectSettings);

            SaveSettings();
        }
    }
}