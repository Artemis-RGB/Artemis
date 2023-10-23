using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Models;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using DryIoc;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public partial class ReleaseWizardViewModel : ActivatableViewModelBase, IWorkshopWizardViewModel
{
    private readonly SubmissionWizardState _state;
    private SubmissionViewModel? _screen;
    [Notify] private bool _shouldClose;

    public ReleaseWizardViewModel(IContainer container, IWindowService windowService, CurrentUserViewModel currentUserViewModel, IGetSubmittedEntryById_Entry entry)
    {
        _state = new SubmissionWizardState(this, container, windowService)
        {
            EntryType = entry.EntryType,
            EntryId = entry.Id
        };

        WindowService = windowService;
        CurrentUserViewModel = currentUserViewModel;
        CurrentUserViewModel.AllowLogout = false;
        Entry = entry;

        _state.StartForCurrentEntry();
    }

    public IWindowService WindowService { get; }
    public IGetSubmittedEntryById_Entry Entry { get; }
    public CurrentUserViewModel CurrentUserViewModel { get; }

    public SubmissionViewModel? Screen
    {
        get => _screen;
        set
        {
            if (value != null)
                value.State = _state;
            RaiseAndSetIfChanged(ref _screen, value);
        }
    }
}