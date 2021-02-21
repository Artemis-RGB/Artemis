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
    }
}