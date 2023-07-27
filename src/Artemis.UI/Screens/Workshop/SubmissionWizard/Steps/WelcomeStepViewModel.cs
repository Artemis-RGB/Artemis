using System.Reactive;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class WelcomeStepViewModel : SubmissionViewModel
{
    #region Overrides of SubmissionViewModel

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> Continue { get; }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> GoBack { get; } = null!;

    public WelcomeStepViewModel()
    {
        ShowGoBack = false;
    }

    #endregion
}