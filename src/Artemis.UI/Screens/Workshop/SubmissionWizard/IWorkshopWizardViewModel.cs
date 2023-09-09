namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public interface IWorkshopWizardViewModel
{
    SubmissionViewModel Screen { get; set; }
    bool ShouldClose { get; set; }
}