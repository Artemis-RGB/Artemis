using Artemis.UI.Installer.Services;

namespace Artemis.UI.Installer.Screens.Steps
{
    public class InstallStepViewModel : ConfigurationStep
    {
        private readonly IInstallationService _installationService;
        private bool _canContinue;

        public InstallStepViewModel(IInstallationService installationService)
        {
            _installationService = installationService;
        }

        public override int Order => 4;

        public bool CanContinue
        {
            get => _canContinue;
            set => SetAndNotify(ref _canContinue, value);
        }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
        }
    }
}