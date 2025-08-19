using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Handlers.InstallationHandlers;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public partial class DefaultEntriesStepViewModel : WizardStepViewModel
{
    [Notify] private bool _workshopReachable;
    [Notify] private bool _fetchingDefaultEntries;
    [Notify] private int _totalEntries;
    [Notify] private int _installedEntries;
    [Notify] private int _installProgress;
    [Notify] private IEntrySummary? _currentEntry;

    private readonly IWorkshopService _workshopService;
    private readonly IWorkshopClient _client;
    private readonly IWindowService _windowService;
    private readonly Progress<StreamProgress> _currentEntryProgress = new();


    public DefaultEntriesStepViewModel(IWorkshopService workshopService, IDeviceService deviceService, IWorkshopClient client, IWindowService windowService)
    {
        _workshopService = workshopService;
        _client = client;
        _windowService = windowService;
        _currentEntryProgress.ProgressChanged += (_, f) => InstallProgress = f.ProgressPercentage;

        Continue = ReactiveCommand.Create(() => Wizard.ChangeScreen<SettingsStepViewModel>());
        GoBack = ReactiveCommand.Create(() =>
        {
            if (deviceService.EnabledDevices.Count == 0)
                Wizard.ChangeScreen<DevicesStepViewModel>();
            else
                Wizard.ChangeScreen<SurfaceStepViewModel>();
        });

        this.WhenActivatedAsync(async d =>
        {
            WorkshopReachable = await _workshopService.ValidateWorkshopStatus(d.AsCancellationToken());
            if (WorkshopReachable)
            {
                await InstallDefaultEntries(d.AsCancellationToken());
            }
        });
    }

    public async Task<bool> InstallDefaultEntries(CancellationToken cancellationToken)
    {
        FetchingDefaultEntries = true;
        TotalEntries = 0;
        InstalledEntries = 0;

        if (!WorkshopReachable)
            return false;

        IOperationResult<IGetDefaultEntriesResult> result = await _client.GetDefaultEntries.ExecuteAsync(100, null, cancellationToken);
        List<IEntrySummary> entries = result.Data?.EntriesV2?.Edges?.Select(e => e.Node).Cast<IEntrySummary>().ToList() ?? [];
        while (result.Data?.EntriesV2?.PageInfo is {HasNextPage: true})
        {
            result = await _client.GetDefaultEntries.ExecuteAsync(100, result.Data.EntriesV2.PageInfo.EndCursor, cancellationToken);
            if (result.Data?.EntriesV2?.Edges != null)
                entries.AddRange(result.Data.EntriesV2.Edges.Select(e => e.Node));
        }

        await Task.Delay(1000);
        FetchingDefaultEntries = false;
        TotalEntries = entries.Count;

        if (entries.Count == 0)
            return false;

        foreach (IEntrySummary entry in entries)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            CurrentEntry = entry;

            // Skip entries without a release and entries that are already installed
            if (entry.LatestRelease == null || _workshopService.GetInstalledEntry(entry.Id) != null)
            {
                InstalledEntries++;
                continue;
            }

            EntryInstallResult installResult = await _workshopService.InstallEntry(entry, entry.LatestRelease, _currentEntryProgress, cancellationToken);

            // Unlikely as default entries do nothing fancy
            if (!installResult.IsSuccess)
            {
                await _windowService.CreateContentDialog().WithTitle("Failed to install entry")
                    .WithContent($"Failed to install entry '{entry.Name}' ({entry.Id}): {installResult.Message}")
                    .WithCloseButtonText("Skip and continue")
                    .ShowAsync();
            }

            InstalledEntries++;
        }

        return true;
    }
}