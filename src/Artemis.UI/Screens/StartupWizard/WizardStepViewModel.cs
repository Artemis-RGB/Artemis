using System.Reactive;
using Artemis.UI.Shared;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard;

public abstract partial class WizardStepViewModel : ValidatableViewModelBase
{
    [Notify] private ReactiveCommand<Unit, Unit>? _secondary;
    [Notify] private ReactiveCommand<Unit, Unit>? _continue;
    [Notify] private ReactiveCommand<Unit, Unit>? _goBack;
    [Notify] private string _continueText = "Continue";
    [Notify] private string? _secondaryText;
    [Notify] private bool _showFinish;
    [Notify] private bool _showGoBack = true;
    [Notify] private bool _showHeader = true;

    public StartupWizardViewModel Wizard { get; set; } = null!;
}