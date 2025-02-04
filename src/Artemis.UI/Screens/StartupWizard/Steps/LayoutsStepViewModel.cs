using Artemis.UI.Screens.Workshop.LayoutFinder;
using ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public class LayoutsStepViewModel : WizardStepViewModel
{
    public LayoutsStepViewModel(LayoutFinderViewModel layoutFinderViewModel)
    {
        LayoutFinderViewModel = layoutFinderViewModel;
        
        Continue = ReactiveCommand.Create(() => Wizard.ChangeScreen<SurfaceStepViewModel>());
        GoBack = ReactiveCommand.Create(() => Wizard.ChangeScreen<DevicesStepViewModel>());
    }

    public LayoutFinderViewModel LayoutFinderViewModel { get; }
}