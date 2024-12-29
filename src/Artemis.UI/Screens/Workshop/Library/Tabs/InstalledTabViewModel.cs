using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using DynamicData;
using DynamicData.Binding;
using DynamicData.List;
using Humanizer;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class InstalledTabViewModel : RoutableScreen
{
    private SourceList<InstalledEntry> _entries = new();

    [Notify] private string? _searchEntryInput;
    private readonly ObservableAsPropertyHelper<bool> _empty;

    public InstalledTabViewModel(IWorkshopService workshopService, IRouter router, Func<InstalledEntry, InstalledTabItemViewModel> getInstalledTabItemViewModel)
    {
        IObservable<Func<InstalledEntry, bool>> searchFilter = this.WhenAnyValue(vm => vm.SearchEntryInput)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(CreatePredicate);

        _entries.Connect()
            .Filter(searchFilter)
            .Sort(SortExpressionComparer<InstalledEntry>.Descending(p => p.InstalledAt))
            .Transform(getInstalledTabItemViewModel)
            .GroupWithImmutableState(vm => vm.Entry.EntryType.Humanize(LetterCasing.Title).Pluralize())
            .Bind(out ReadOnlyObservableCollection<IGrouping<InstalledTabItemViewModel, string>> entryViewModels)
            .Subscribe();
        _empty = _entries.Connect().Count().Select(c => c == 0).ToProperty(this, vm => vm.Empty);
        _entries.AddRange(workshopService.GetInstalledEntries());
        
        EntryGroups = entryViewModels;

        this.WhenActivated(d =>
        {
            workshopService.OnEntryUninstalled += WorkshopServiceOnOnEntryUninstalled;
            Disposable.Create(() => workshopService.OnEntryUninstalled -= WorkshopServiceOnOnEntryUninstalled).DisposeWith(d);
        });

        OpenWorkshop = ReactiveCommand.CreateFromTask(async () => await router.Navigate("workshop"));
    }

    private void WorkshopServiceOnOnEntryUninstalled(object? sender, InstalledEntry e)
    {
        _entries.Remove(e);
    }

    public bool Empty => _empty.Value;
    public ReactiveCommand<Unit, Unit> OpenWorkshop { get; }
    public ReadOnlyObservableCollection<IGrouping<InstalledTabItemViewModel, string>> EntryGroups { get; }

    private Func<InstalledEntry, bool> CreatePredicate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return _ => true;

        return data => data.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase);
    }
}