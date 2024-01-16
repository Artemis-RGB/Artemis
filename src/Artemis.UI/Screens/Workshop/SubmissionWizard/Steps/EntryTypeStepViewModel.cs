using System.Reactive.Linq;
using Artemis.WebClient.Workshop;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public partial class EntryTypeStepViewModel : SubmissionViewModel
{
    [Notify] private EntryType? _selectedEntryType;

    /// <inheritdoc />
    public EntryTypeStepViewModel()
    {
        GoBack = ReactiveCommand.Create(() => State.ChangeScreen<WelcomeStepViewModel>());
        Continue = ReactiveCommand.Create(ExecuteContinue, this.WhenAnyValue(vm => vm.SelectedEntryType).Select(e => e != null));
    }

    private void ExecuteContinue()
    {
        if (SelectedEntryType == null)
            return;

        State.EntryType = SelectedEntryType.Value;
        State.StartForCurrentEntry();
    }
}