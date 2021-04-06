using Artemis.Core.Services;
using Stylet;

namespace Artemis.UI.Screens.StartupWizard.Steps
{
    public class LayoutStepViewModel : Screen
    {
        private readonly IRgbService _rgbService;

        public LayoutStepViewModel(IRgbService rgbService)
        {
            _rgbService = rgbService;
        }

        public void ApplyLeftHandedPreset()
        {
            _rgbService.AutoArrangeDevices();
            StartupWizardViewModel startupWizardViewModel = (StartupWizardViewModel) Parent;
            startupWizardViewModel.Continue();
        }

        public void ApplyRightHandedPreset()
        {
            _rgbService.AutoArrangeDevices();
            StartupWizardViewModel startupWizardViewModel = (StartupWizardViewModel) Parent;
            startupWizardViewModel.Continue();
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnActivate()
        {
            _rgbService.IsRenderPaused = true;
            base.OnActivate();
        }

        /// <inheritdoc />
        protected override void OnDeactivate()
        {
            _rgbService.IsRenderPaused = false;
            base.OnDeactivate();
        }

        #endregion
    }
}