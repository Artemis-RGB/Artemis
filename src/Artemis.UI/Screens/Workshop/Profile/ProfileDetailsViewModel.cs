using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.DownloadHandlers;
using Artemis.WebClient.Workshop.DownloadHandlers.Implementations;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfileDetailsViewModel : RoutableScreen<ActivatableViewModelBase, WorkshopDetailParameters>, IWorkshopViewModel
{
    private readonly IWorkshopClient _client;
    private readonly ProfileEntryDownloadHandler _downloadHandler;
    private readonly INotificationService _notificationService;
    private readonly ObservableAsPropertyHelper<DateTimeOffset?> _updatedAt;
    private IGetEntryById_Entry? _entry;

    public ProfileDetailsViewModel(IWorkshopClient client, ProfileEntryDownloadHandler downloadHandler, INotificationService notificationService)
    {
        _client = client;
        _downloadHandler = downloadHandler;
        _notificationService = notificationService;
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

    private async Task ExecuteDownloadLatestRelease(CancellationToken cancellationToken)
    {
        if (Entry?.LatestRelease == null)
            return;

        EntryInstallResult<ProfileConfiguration> result = await _downloadHandler.InstallProfileAsync(Entry.LatestRelease.Id, new Progress<StreamProgress>(), cancellationToken);
        if (result.IsSuccess)
            _notificationService.CreateNotification().WithTitle("Profile installed").WithSeverity(NotificationSeverity.Success).Show();
        else
            _notificationService.CreateNotification().WithTitle("Failed to install profile").WithMessage(result.Message).WithSeverity(NotificationSeverity.Error).Show();
    }

    public EntryType? EntryType => null;
}