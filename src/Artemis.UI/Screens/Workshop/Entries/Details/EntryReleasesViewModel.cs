using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Handlers.InstallationHandlers;
using Artemis.WebClient.Workshop.Models;
using Humanizer;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public class EntryReleasesViewModel : ViewModelBase
{
    private readonly EntryInstallationHandlerFactory _factory;
    private readonly IWindowService _windowService;
    private readonly INotificationService _notificationService;
    private readonly IRouter _router;

    public EntryReleasesViewModel(IEntryDetails entry, EntryInstallationHandlerFactory factory, IWindowService windowService, INotificationService notificationService, IRouter router)
    {
        _factory = factory;
        _windowService = windowService;
        _notificationService = notificationService;
        _router = router;

        Entry = entry;
        LatestRelease = Entry.Releases.MaxBy(r => r.CreatedAt);
        OtherReleases = Entry.Releases.OrderByDescending(r => r.CreatedAt).Skip(1).Take(4).Cast<IRelease>().ToList();

        DownloadLatestRelease = ReactiveCommand.CreateFromTask(ExecuteDownloadLatestRelease);
        OnInstallationStarted = Confirm;
        NavigateToRelease = ReactiveCommand.CreateFromTask<IRelease>(ExecuteNavigateToRelease);
    }

    public IEntryDetails Entry { get; }
    public IRelease? LatestRelease { get; }
    public List<IRelease> OtherReleases { get; }

    public ReactiveCommand<Unit, Unit> DownloadLatestRelease { get; }
    public ReactiveCommand<IRelease, Unit> NavigateToRelease { get; }
    
    public Func<IEntryDetails, IRelease, Task<bool>> OnInstallationStarted { get; set; }
    public Func<InstalledEntry, Task>? OnInstallationFinished { get; set; }

    private async Task ExecuteNavigateToRelease(IRelease release)
    {
        await _router.Navigate($"workshop/entries/{Entry.Id}/releases/{release.Id}");
    }

    private async Task ExecuteDownloadLatestRelease(CancellationToken cancellationToken)
    {
        if (LatestRelease == null)
            return;

        if (await OnInstallationStarted(Entry, LatestRelease))
            return;

        IEntryInstallationHandler installationHandler = _factory.CreateHandler(Entry.EntryType);
        EntryInstallResult result = await installationHandler.InstallAsync(Entry, LatestRelease, new Progress<StreamProgress>(), cancellationToken);
        if (result.IsSuccess && result.Entry != null)
        {
            if (OnInstallationFinished != null)
                await OnInstallationFinished(result.Entry);
            _notificationService.CreateNotification().WithTitle($"{Entry.EntryType.Humanize(LetterCasing.Sentence)} installed").WithSeverity(NotificationSeverity.Success).Show();
        }
        else
        {
            _notificationService.CreateNotification()
                .WithTitle($"Failed to install {Entry.EntryType.Humanize(LetterCasing.LowerCase)}")
                .WithMessage(result.Message)
                .WithSeverity(NotificationSeverity.Error).Show();
        }
    }

    private async Task<bool> Confirm(IEntryDetails entryDetails, IRelease release)
    {
        bool confirm = await _windowService.ShowConfirmContentDialog(
            "Install latest release",
            $"Are you sure you want to download and install version {release.Version} of {entryDetails.Name}?"
        );

        return !confirm;
    }
}