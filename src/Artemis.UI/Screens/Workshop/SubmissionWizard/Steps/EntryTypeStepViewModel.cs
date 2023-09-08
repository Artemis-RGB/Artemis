using System.Reactive.Linq;
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
    
#if DEBUG
    public bool ShowLayouts => true;
# else
    public bool ShowLayouts => false;
#endif

    public EntryType? SelectedEntryType
    {
        get => _selectedEntryType;
        set => RaiseAndSetIfChanged(ref _selectedEntryType, value);
    }

    private void ExecuteContinue()
    {
        if (SelectedEntryType == null)
            return;

        State.EntryType = SelectedEntryType.Value;
        State.StartForCurrentEntry();
    }
}