using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using DryIoc;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public class SubmissionWizardViewModel : DialogViewModelBase<bool>
{
    private readonly SubmissionWizardState _state;
    private SubmissionViewModel _screen;

    public SubmissionWizardViewModel(IContainer container, IWindowService windowService, CurrentUserViewModel currentUserViewModel, WelcomeStepViewModel welcomeStepViewModel)
    {
        _state = new SubmissionWizardState(this, container, windowService);
        _screen = welcomeStepViewModel;
        _screen.WizardViewModel = this;
        _screen.State = _state;

        WindowService = windowService;
        CurrentUserViewModel = currentUserViewModel;
        CurrentUserViewModel.AllowLogout = false;
    }

    public IWindowService WindowService { get; }
    public CurrentUserViewModel CurrentUserViewModel { get; }

    public SubmissionViewModel Screen
    {
        get => _screen;
        set
        {
            value.WizardViewModel = this;
            value.State = _state;
            RaiseAndSetIfChanged(ref _screen, value);
        }
    }
}