using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;
using Artemis.UI.Shared;
using DryIoc;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public class SubmissionWizardViewModel : DialogViewModelBase<bool>
{
    private readonly SubmissionWizardState _state;
    private SubmissionViewModel _screen;

    public SubmissionWizardViewModel(IContainer container, CurrentUserViewModel currentUserViewModel, WelcomeStepViewModel welcomeStepViewModel)
    {
        _state = new SubmissionWizardState(this, container);
        _screen = welcomeStepViewModel;
        _screen.WizardViewModel = this;
        _screen.State = _state;
        
        CurrentUserViewModel = currentUserViewModel;
        CurrentUserViewModel.AllowLogout = false;
    }

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