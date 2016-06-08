using Artemis.Managers;
using Artemis.Models;
using Artemis.Services;
using Caliburn.Micro;
using Ninject;

namespace Artemis.ViewModels.Abstract
{
    public abstract class OverlayViewModel : Screen
    {
        protected readonly MainManager MainManager;
        private OverlaySettings _overlaySettings;

        protected OverlayViewModel(MainManager mainManager)
        {
            MainManager = mainManager;
        }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public OverlayModel OverlayModel { get; set; }

        public OverlaySettings OverlaySettings
        {
            get { return _overlaySettings; }
            set
            {
                if (Equals(value, _overlaySettings)) return;
                _overlaySettings = value;
                NotifyOfPropertyChange(() => OverlaySettings);
            }
        }

        public void ToggleOverlay()
        {
            OverlayModel.Enabled = OverlaySettings.Enabled;
        }

        public void SaveSettings()
        {
            OverlaySettings?.Save();
        }

        public async void ResetSettings()
        {
            var resetConfirm = await
                DialogService.ShowQuestionMessageBox("Reset overlay settings",
                    "Are you sure you wish to reset this overlay's settings? \nAny changes you made will be lost.");

            if (!resetConfirm.Value)
                return;

            OverlaySettings.ToDefault();
            NotifyOfPropertyChange(() => OverlaySettings);

            OverlayModel.Enabled = OverlaySettings.Enabled;
            SaveSettings();
        }
    }
}