using Artemis.UI.Installer.Services;

namespace Artemis.UI.Installer.Screens.Steps
{
    public class WelcomeStepViewModel : ConfigurationStep
    {
        private readonly IInstallationService _installationService;

        public WelcomeStepViewModel(IInstallationService installationService)
        {
            _installationService = installationService;
        }

        public override int Order => 1;
    }
}