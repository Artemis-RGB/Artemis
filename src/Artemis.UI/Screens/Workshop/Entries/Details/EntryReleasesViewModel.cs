using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
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

    public EntryReleasesViewModel(IGetEntryById_Entry entry, EntryInstallationHandlerFactory factory, IWindowService windowService, INotificationService notificationService)
    {
        _factory = factory;
        _windowService = windowService;
        _notificationService = notificationService;

        Entry = entry;
        DownloadLatestRelease = ReactiveCommand.CreateFromTask(ExecuteDownloadLatestRelease);
        OnInstallationStarted = Confirm;
    }

    public IGetEntryById_Entry Entry { get; }
    public ReactiveCommand<Unit, Unit> DownloadLatestRelease { get; }

    public Func<IEntryDetails, Task<bool>> OnInstallationStarted { get; set; }
    public Func<InstalledEntry, Task>? OnInstallationFinished { get; set; }

    private async Task ExecuteDownloadLatestRelease(CancellationToken cancellationToken)
    {
        if (Entry.LatestRelease == null)
            return;
        
        if (await OnInstallationStarted(Entry))
            return;

        IEntryInstallationHandler installationHandler = _factory.CreateHandler(Entry.EntryType);
        EntryInstallResult result = await installationHandler.InstallAsync(Entry, Entry.LatestRelease, new Progress<StreamProgress>(), cancellationToken);
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

    private async Task<bool> Confirm(IEntryDetails entryDetails)
    {
        bool confirm = await _windowService.ShowConfirmContentDialog(
            "Install latest release",
            $"Are you sure you want to download and install version {entryDetails.LatestRelease?.Version} of {entryDetails.Name}?"
        );
        
        return !confirm;
    }
}