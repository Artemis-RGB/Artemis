using System.Reactive;
using Artemis.UI.Shared;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public abstract partial class SubmissionViewModel : ValidatableViewModelBase
{
    [Notify] private ReactiveCommand<Unit, Unit>? _continue;
    [Notify] private ReactiveCommand<Unit, Unit>? _goBack;
    [Notify] private string _continueText = "Continue";
    [Notify] private bool _showFinish;
    [Notify] private bool _showGoBack = true;
    [Notify] private bool _showHeader = true;

    public SubmissionWizardState State { get; set; } = null!;
}