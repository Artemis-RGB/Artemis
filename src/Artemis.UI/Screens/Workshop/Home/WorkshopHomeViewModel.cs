using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Screens.Workshop.SubmissionWizard;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using DynamicData;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Home;

public partial class WorkshopHomeViewModel : RoutableScreen
{
    private readonly IWindowService _windowService;

    [Notify(Setter.Private)] private bool _workshopReachable;

    public WorkshopHomeViewModel(IRouter router, IWindowService windowService, IWorkshopService workshopService, IWorkshopClient client,
        Func<IEntrySummary, EntryListItemVerticalViewModel> getEntryListItemViewModel)
    {
        _windowService = windowService;
        SourceList<IEntrySummary> latest = new();
        SourceList<IEntrySummary> popular = new();

        latest.Connect().Transform(getEntryListItemViewModel).Bind(out ReadOnlyObservableCollection<EntryListItemVerticalViewModel> latestEntries).Subscribe();
        popular.Connect().Transform(getEntryListItemViewModel).Bind(out ReadOnlyObservableCollection<EntryListItemVerticalViewModel> popularEntries).Subscribe();

        AddSubmission = ReactiveCommand.CreateFromTask(ExecuteAddSubmission, this.WhenAnyValue(vm => vm.WorkshopReachable));
        Navigate = ReactiveCommand.CreateFromTask<string>(async r => await router.Navigate(r), this.WhenAnyValue(vm => vm.WorkshopReachable));
        PopularEntries = popularEntries;
        LatestEntries = latestEntries;

        this.WhenActivatedAsync(async d =>
        {
            WorkshopReachable = await workshopService.ValidateWorkshopStatus(d.AsCancellationToken());

            IOperationResult<IGetPopularEntriesResult> popularResult = await client.GetPopularEntries.ExecuteAsync();
            popular.Edit(p =>
            {
                p.Clear();
                if (popularResult.Data?.PopularEntries != null)
                    p.AddRange(popularResult.Data.PopularEntries.Take(8));
            });

            IOperationResult<IGetEntriesv2Result> latestResult = await client.GetEntriesv2.ExecuteAsync(null, null, [new EntrySortInput {CreatedAt = SortEnumType.Desc}], 8, null);
            latest.Edit(l =>
            {
                l.Clear();
                if (latestResult.Data?.EntriesV2?.Edges != null)
                    l.AddRange(latestResult.Data.EntriesV2.Edges.Select(e => e.Node));
            });
        });
    }

    public ReactiveCommand<Unit, Unit> AddSubmission { get; }
    public ReactiveCommand<string, Unit> Navigate { get; }
    public ReadOnlyObservableCollection<EntryListItemVerticalViewModel> PopularEntries { get; }
    public ReadOnlyObservableCollection<EntryListItemVerticalViewModel> LatestEntries { get; }

    private async Task ExecuteAddSubmission(CancellationToken arg)
    {
        await _windowService.ShowDialogAsync<SubmissionWizardViewModel>();
    }
}