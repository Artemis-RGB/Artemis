using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Humanizer;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.EntryReleases.Dialogs;

public class DependenciesDialogViewModel : ContentDialogViewModelBase
{
    public DependenciesDialogViewModel(IEntrySummary dependant, List<IEntrySummary> dependencies, Func<IEntrySummary, EntryListItemViewModel> getEntryListItemViewModel, IRouter router)
    {
        Dependant = dependant;
        DependantType = dependant.EntryType.Humanize(LetterCasing.LowerCase);
        EntryType = dependencies.First().EntryType.Humanize(LetterCasing.LowerCase);
        EntryTypePlural = dependencies.First().EntryType.Humanize(LetterCasing.LowerCase).Pluralize();
        Dependencies = new ObservableCollection<EntryListItemViewModel>(dependencies.Select(getEntryListItemViewModel));

        this.WhenActivated(d => router.CurrentPath.Skip(1).Subscribe(s => ContentDialog?.Hide()).DisposeWith(d));
    }

    public string DependantType { get; }
    public string EntryType { get; }
    public string EntryTypePlural { get; }
    public bool Multiple => Dependencies.Count > 1;

    public IEntrySummary Dependant { get; }
    public ObservableCollection<EntryListItemViewModel> Dependencies { get; }
}