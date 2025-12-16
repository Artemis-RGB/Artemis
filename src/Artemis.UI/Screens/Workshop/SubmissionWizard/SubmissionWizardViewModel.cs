using System.Reactive.Disposables.Fluent;
using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Models;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using DryIoc;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public partial class SubmissionWizardViewModel : ActivatableViewModelBase, IWorkshopWizardViewModel
{
    private readonly SubmissionWizardState _state;
    private SubmissionViewModel _screen;
    [Notify] private bool _shouldClose;

    public SubmissionWizardViewModel(IContainer container,
        IWindowService windowService,
        CurrentUserViewModel currentUserViewModel,
        WelcomeStepViewModel welcomeStepViewModel)
    {
        _state = new SubmissionWizardState(this, container, windowService);
        _screen = welcomeStepViewModel;
        _screen.State = _state;

        WindowService = windowService;
        CurrentUserViewModel = currentUserViewModel;
        CurrentUserViewModel.AllowLogout = false;
        
        this.WhenActivated(d => _state.DisposeWith(d));
    }

    public IWindowService WindowService { get; }
    public CurrentUserViewModel CurrentUserViewModel { get; }

    public SubmissionViewModel Screen
    {
        get => _screen;
        set
        {
            value.State = _state;
            RaiseAndSetIfChanged(ref _screen, value);
        }
    }
}