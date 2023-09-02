using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.DownloadHandlers;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Layout;

public class LayoutDetailsViewModel : RoutableScreen<WorkshopDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly INotificationService _notificationService;
    private readonly IWindowService _windowService;
    private readonly ObservableAsPropertyHelper<DateTimeOffset?> _updatedAt;
    private IGetEntryById_Entry? _entry;

    public LayoutDetailsViewModel(IWorkshopClient client, INotificationService notificationService, IWindowService windowService)
    {
        _client = client;
        _notificationService = notificationService;
        _windowService = windowService;
        _updatedAt = this.WhenAnyValue(vm => vm.Entry).Select(e => e?.LatestRelease?.CreatedAt ?? e?.CreatedAt).ToProperty(this, vm => vm.UpdatedAt);

        DownloadLatestRelease = ReactiveCommand.CreateFromTask(ExecuteDownloadLatestRelease);
    }

    public ReactiveCommand<Unit, Unit> DownloadLatestRelease { get; }

    public DateTimeOffset? UpdatedAt => _updatedAt.Value;

    public IGetEntryById_Entry? Entry
    {
        get => _entry;
        private set => RaiseAndSetIfChanged(ref _entry, value);
    }

    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        await GetEntry(parameters.EntryId, cancellationToken);
    }

    private async Task GetEntry(Guid entryId, CancellationToken cancellationToken)
    {
        IOperationResult<IGetEntryByIdResult> result = await _client.GetEntryById.ExecuteAsync(entryId, cancellationToken);
        if (result.IsErrorResult())
            return;

        Entry = result.Data?.Entry;
    }

    private Task ExecuteDownloadLatestRelease(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}