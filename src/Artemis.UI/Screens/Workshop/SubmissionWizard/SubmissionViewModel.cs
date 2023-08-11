using System.Reactive;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public abstract class SubmissionViewModel : ValidatableViewModelBase
{
    private string _continueText = "Continue";
    private bool _showFinish;
    private bool _showGoBack = true;
    private bool _showHeader = true;

    public SubmissionWizardViewModel WizardViewModel { get; set; } = null!;
    public SubmissionWizardState State { get; set; } = null!;
    
    public abstract ReactiveCommand<Unit, Unit> Continue { get; }
    public abstract ReactiveCommand<Unit, Unit> GoBack { get; }

    public bool ShowHeader
    {
        get => _showHeader;
        set => RaiseAndSetIfChanged(ref _showHeader, value);
    }

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

    public string ContinueText
    {
        get => _continueText;
        set => RaiseAndSetIfChanged(ref _continueText, value);
    }
}