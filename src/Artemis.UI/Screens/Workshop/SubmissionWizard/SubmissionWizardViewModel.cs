using System.Reactive;
using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public class SubmissionWizardViewModel : DialogViewModelBase<bool>
{
    private SubmissionViewModel _screen;

    public SubmissionWizardViewModel(CurrentUserViewModel currentUserViewModel)
    {
        _screen = new WelcomeStepViewModel();
        CurrentUserViewModel = currentUserViewModel;
    }

    public CurrentUserViewModel CurrentUserViewModel { get; }

    public SubmissionViewModel Screen
    {
        get => _screen;
        set => RaiseAndSetIfChanged(ref _screen, value);
    }
}

public abstract class SubmissionViewModel : ActivatableViewModelBase
{
    private bool _showFinish;
    private bool _showGoBack = true;

    public abstract ReactiveCommand<Unit, Unit> Continue { get; }
    public abstract ReactiveCommand<Unit, Unit> GoBack { get; }

    public bool ShowGoBack
    {
        get => _showGoBack;
        set => RaiseAndSetIfChanged(ref _showGoBack, value);
    }

    public bool ShowFinish
    {
        get => _showFinish;
        set => RaiseAndSetIfChanged(ref _showFinish, value);
    }
}