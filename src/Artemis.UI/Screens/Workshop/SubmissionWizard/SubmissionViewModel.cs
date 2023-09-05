using System.Reactive;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public abstract class SubmissionViewModel : ValidatableViewModelBase
{
    private ReactiveCommand<Unit, Unit>? _continue;
    private ReactiveCommand<Unit, Unit>? _goBack;
    private string _continueText = "Continue";
    private bool _showFinish;
    private bool _showGoBack = true;
    private bool _showHeader = true;

    public SubmissionWizardState State { get; set; } = null!;

    public ReactiveCommand<Unit, Unit>? Continue
    {
        get => _continue;
        set => RaiseAndSetIfChanged(ref _continue, value);
    }

    public ReactiveCommand<Unit, Unit>? GoBack
    {
        get => _goBack;
        set => RaiseAndSetIfChanged(ref _goBack, value);
    }

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