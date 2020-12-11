using Artemis.Core.Services;
using Stylet;

namespace Artemis.UI.Screens.SetupWizard.Steps
{
    public class LayoutStepViewModel : Screen
    {
        private readonly ISurfaceService _surfaceService;

        public LayoutStepViewModel(ISurfaceService surfaceService)
        {
            _surfaceService = surfaceService;
        }

        public void ApplyLeftHandedPreset()
        {
            _surfaceService.AutoArrange();
            SetupWizardViewModel setupWizardViewModel = (SetupWizardViewModel) Parent;
            setupWizardViewModel.Continue();
        }

        public void ApplyRightHandedPreset()
        {
            _surfaceService.AutoArrange();
            SetupWizardViewModel setupWizardViewModel = (SetupWizardViewModel) Parent;
            setupWizardViewModel.Continue();
        }
    }
}