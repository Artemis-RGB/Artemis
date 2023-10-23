using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using Artemis.UI.Screens.Workshop.Layout;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Models;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Layout;

public class LayoutInfoStepViewModel : SubmissionViewModel
{
    public LayoutInfoStepViewModel()
    {
        GoBack = ReactiveCommand.Create(() => State.ChangeScreen<LayoutSelectionStepViewModel>());
        this.WhenActivated((CompositeDisposable _) =>
        {
            LayoutInfo.Clear();
            if (State.EntrySource is LayoutEntrySource layoutEntrySource)
                LayoutInfo.AddRange(layoutEntrySource.LayoutInfo.Select(i => new LayoutInfoViewModel(layoutEntrySource.Layout, i)));
        });
    }

    public ObservableCollection<LayoutInfoViewModel> LayoutInfo { get; } = new();
}