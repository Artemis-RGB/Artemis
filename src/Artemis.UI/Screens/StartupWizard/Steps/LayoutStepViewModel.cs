using Artemis.Core.Services;
using Stylet;

namespace Artemis.UI.Screens.StartupWizard.Steps
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
            StartupWizardViewModel startupWizardViewModel = (StartupWizardViewModel) Parent;
            startupWizardViewModel.Continue();
        }

        public void ApplyRightHandedPreset()
        {
            _surfaceService.AutoArrange();
            StartupWizardViewModel startupWizardViewModel = (StartupWizardViewModel) Parent;
            startupWizardViewModel.Continue();
        }
    }
}