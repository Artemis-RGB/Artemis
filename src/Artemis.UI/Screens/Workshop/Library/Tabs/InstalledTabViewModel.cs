using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop.Services;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public class InstalledTabViewModel : RoutableScreen
{
    private string? _searchEntryInput;

    public InstalledTabViewModel(IWorkshopService workshopService, IRouter router, Func<InstalledEntry, InstalledTabItemViewModel> getInstalledTabItemViewModel)
    {
        SourceList<InstalledEntry> installedEntries = new();
        IObservable<Func<InstalledEntry, bool>> pluginFilter = this.WhenAnyValue(vm => vm.SearchEntryInput).Throttle(TimeSpan.FromMilliseconds(100)).Select(CreatePredicate);

        installedEntries.Connect()
            .Filter(pluginFilter)
            .Sort(SortExpressionComparer<InstalledEntry>.Descending(p => p.InstalledAt))
            .Transform(getInstalledTabItemViewModel)
            .AutoRefresh(vm => vm.IsRemoved)
            .Filter(vm => !vm.IsRemoved)
            .Bind(out ReadOnlyObservableCollection<InstalledTabItemViewModel> installedEntryViewModels)
            .Subscribe();
        
        List<InstalledEntry> entries = workshopService.GetInstalledEntries();
        installedEntries.AddRange(entries);
        
        Empty = entries.Count == 0;
        InstalledEntries = installedEntryViewModels;
        
        OpenWorkshop = ReactiveCommand.CreateFromTask(async () => await router.Navigate("workshop"));
    }

    public bool Empty { get; }
    public ReactiveCommand<Unit, Unit> OpenWorkshop { get; }

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

        return data => data.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase);
    }
}