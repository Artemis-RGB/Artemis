using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class InstalledTabViewModel : RoutableScreen
{
    private SourceList<InstalledEntry> _installedEntries = new();
    
    [Notify] private string? _searchEntryInput;

    public InstalledTabViewModel(IWorkshopService workshopService, IRouter router, Func<InstalledEntry, InstalledTabItemViewModel> getInstalledTabItemViewModel)
    {
        IObservable<Func<InstalledEntry, bool>> pluginFilter = this.WhenAnyValue(vm => vm.SearchEntryInput).Throttle(TimeSpan.FromMilliseconds(100)).Select(CreatePredicate);

        _installedEntries.Connect()
            .Filter(pluginFilter)
            .Sort(SortExpressionComparer<InstalledEntry>.Descending(p => p.InstalledAt))
            .Transform(getInstalledTabItemViewModel)
            .Bind(out ReadOnlyObservableCollection<InstalledTabItemViewModel> installedEntryViewModels)
            .Subscribe();
        
        List<InstalledEntry> entries = workshopService.GetInstalledEntries();
        _installedEntries.AddRange(entries);
        
        Empty = entries.Count == 0;
        InstalledEntries = installedEntryViewModels;
        
        this.WhenActivated(d =>
        {
            workshopService.OnEntryUninstalled += WorkshopServiceOnOnEntryUninstalled;
            Disposable.Create(() => workshopService.OnEntryUninstalled -= WorkshopServiceOnOnEntryUninstalled).DisposeWith(d);
        });
        
        OpenWorkshop = ReactiveCommand.CreateFromTask(async () => await router.Navigate("workshop"));
    }

    private void WorkshopServiceOnOnEntryUninstalled(object? sender, InstalledEntry e)
    {
        _installedEntries.Remove(e);
    }

    public bool Empty { get; }
    public ReactiveCommand<Unit, Unit> OpenWorkshop { get; }
    public ReadOnlyObservableCollection<InstalledTabItemViewModel> InstalledEntries { get; }
    
    private Func<InstalledEntry, bool> CreatePredicate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return _ => true;

        return data => data.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase);
    }
}