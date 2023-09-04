using System.Reactive;
using System.Reactive.Linq;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;
using Artemis.WebClient.Workshop;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class EntryTypeStepViewModel : SubmissionViewModel
{
    private EntryType? _selectedEntryType;

    /// <inheritdoc />
    public EntryTypeStepViewModel()
    {
        GoBack = ReactiveCommand.Create(() => State.ChangeScreen<WelcomeStepViewModel>());
        Continue = ReactiveCommand.Create(ExecuteContinue, this.WhenAnyValue(vm => vm.SelectedEntryType).Select(e => e != null));
    }

    public EntryType? SelectedEntryType
    {
        get => _selectedEntryType;
        set => RaiseAndSetIfChanged(ref _selectedEntryType, value);
    }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> Continue { get; }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> GoBack { get; }

    private void ExecuteContinue()
    {
        if (SelectedEntryType == null)
            return;

        State.EntryType = SelectedEntryType.Value;
        State.StartForCurrentEntry();
    }
}