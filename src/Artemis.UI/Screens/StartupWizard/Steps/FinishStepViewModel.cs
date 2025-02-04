using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public class FinishStepViewModel : WizardStepViewModel
{
    public FinishStepViewModel()
    {
        GoBack = ReactiveCommand.Create(() => Wizard.ChangeScreen<SettingsStepViewModel>());
        ShowFinish = true;
    }
}