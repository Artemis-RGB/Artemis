using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop.Services;
using Avalonia.ReactiveUI;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public class InstalledTabViewModel : RoutableScreen
{
    private string? _searchEntryInput;

    public InstalledTabViewModel(IWorkshopService workshopService, Func<InstalledEntry, InstalledTabItemViewModel> getInstalledTabItemViewModel)
    {
        SourceList<InstalledEntry> installedEntries = new();
        IObservable<Func<InstalledEntry, bool>> pluginFilter = this.WhenAnyValue(vm => vm.SearchEntryInput).Throttle(TimeSpan.FromMilliseconds(100)).Select(CreatePredicate);

        installedEntries.Connect()
            .Filter(pluginFilter)
            .Sort(SortExpressionComparer<InstalledEntry>.Ascending(p => p.Name))
            .Transform(getInstalledTabItemViewModel)
            .ObserveOn(AvaloniaScheduler.Instance)
            .Bind(out ReadOnlyObservableCollection<InstalledTabItemViewModel> installedEntryViewModels)
            .Subscribe();
        InstalledEntries = installedEntryViewModels;
        
        this.WhenActivated(d =>
        {
            installedEntries.AddRange(workshopService.GetInstalledEntries());
            Disposable.Create(installedEntries, e => e.Clear()).DisposeWith(d);
        });
    }

    public ReadOnlyObservableCollection<InstalledTabItemViewModel> InstalledEntries { get; }

    public string? SearchEntryInput
    {
        get => _searchEntryInput;
        set => RaiseAndSetIfChanged(ref _searchEntryInput, value);
    }

    private Func<InstalledEntry, bool> CreatePredicate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return _ => true;

        return data => data.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase) ||
                       data.Summary.Contains(text, StringComparison.InvariantCultureIgnoreCase);
    }
}