using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Screens.Workshop.LayoutFinder;
using ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public class LayoutsStepViewModel : WizardStepViewModel
{
    public LayoutsStepViewModel(LayoutFinderViewModel layoutFinderViewModel)
    {
        LayoutFinderViewModel = layoutFinderViewModel;

        Continue = ReactiveCommand.Create(() => Wizard.ChangeScreen<SurfaceStepViewModel>(), LayoutFinderViewModel.SearchAll.IsExecuting.Select(isExecuting => !isExecuting));
        GoBack = ReactiveCommand.Create(() => Wizard.ChangeScreen<DefaultEntriesStepViewModel>(), LayoutFinderViewModel.SearchAll.IsExecuting.Select(isExecuting => !isExecuting));

        LayoutFinderViewModel.WhenActivated((CompositeDisposable _) => LayoutFinderViewModel.SearchAll.Execute().Subscribe());
    }
    
    public LayoutFinderViewModel LayoutFinderViewModel { get; }
}