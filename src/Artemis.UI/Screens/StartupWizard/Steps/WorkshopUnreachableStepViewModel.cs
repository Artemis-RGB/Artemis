namespace Artemis.UI.Screens.StartupWizard.Steps;

public class WorkshopUnreachableStepViewModel : WizardStepViewModel
{
    public WorkshopUnreachableStepViewModel()
    {
        HideAllButtons = true;
    }

    public void Retry()
    {
        Wizard.ChangeScreen<DefaultEntriesStepViewModel>();
    }

    public void Skip()
    {
        Wizard.Close(false);
    }
}