using ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public class WelcomeStepViewModel : WizardStepViewModel
{
    public WelcomeStepViewModel()
    {
        Continue = ReactiveCommand.Create(() => Wizard.ChangeScreen<DevicesStepViewModel>());
        ShowGoBack = false;
    }
}