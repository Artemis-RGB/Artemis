using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public partial class DefaultEntriesStepViewModel : WizardStepViewModel
{
    [Notify] private bool _workshopReachable;
    [Notify] private bool _fetchingDefaultEntries;
    [Notify] private bool _installed;
    [Notify] private int _currentEntries;
    [Notify] private int _totalEntries = 1;

    private readonly IWorkshopClient _client;
    private readonly Func<IEntrySummary, DefaultEntryItemViewModel> _getDefaultEntryItemViewModel;

    public ObservableCollection<DefaultEntryItemViewModel> DeviceProviderEntryViewModels { get; } = [];
    public ObservableCollection<DefaultEntryItemViewModel> EssentialEntryViewModels { get; } = [];
    public ObservableCollection<DefaultEntryItemViewModel> OtherEntryViewModels { get; } = [];

    public DefaultEntriesStepViewModel(IWorkshopService workshopService, IDeviceService deviceService, IWorkshopClient client,
        Func<IEntrySummary, DefaultEntryItemViewModel> getDefaultEntryItemViewModel)
    {
        _client = client;
        _getDefaultEntryItemViewModel = getDefaultEntryItemViewModel;

        ContinueText = "Install selected entries";
        Continue = ReactiveCommand.CreateFromTask(async ct =>
        {
            if (Installed)
                Wizard.ChangeScreen<SettingsStepViewModel>();
            else
                await Install(ct);
        });
        GoBack = ReactiveCommand.Create(() =>
        {
            if (deviceService.EnabledDevices.Count == 0)
                Wizard.ChangeScreen<DevicesStepViewModel>();
            else
                Wizard.ChangeScreen<SurfaceStepViewModel>();
        });

        this.WhenActivatedAsync(async d =>
        {
            WorkshopReachable = await workshopService.ValidateWorkshopStatus(d.AsCancellationToken());
            if (WorkshopReachable)
            {
                await GetDefaultEntries(d.AsCancellationToken());
            }
        });
    }

    private async Task Install(CancellationToken cancellationToken)
    {
        // Remove entries that aren't to be installed
        RemoveUnselectedEntries(DeviceProviderEntryViewModels);
        RemoveUnselectedEntries(EssentialEntryViewModels);
        RemoveUnselectedEntries(OtherEntryViewModels);
        
        TotalEntries = DeviceProviderEntryViewModels.Count + EssentialEntryViewModels.Count + OtherEntryViewModels.Count;
        CurrentEntries = 0;

        // Install entries one by one, removing them from the list as we go
        List<DefaultEntryItemViewModel> entries = [..DeviceProviderEntryViewModels, ..EssentialEntryViewModels, ..OtherEntryViewModels];
            foreach (DefaultEntryItemViewModel defaultEntryItemViewModel in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool removeFromList = await defaultEntryItemViewModel.InstallEntry(cancellationToken);
            if (!removeFromList)
                break;

            DeviceProviderEntryViewModels.Remove(defaultEntryItemViewModel);
            EssentialEntryViewModels.Remove(defaultEntryItemViewModel);
            OtherEntryViewModels.Remove(defaultEntryItemViewModel);
            CurrentEntries++;
        }

        Installed = true;
        ContinueText = "Continue";
    }

    private void RemoveUnselectedEntries(ObservableCollection<DefaultEntryItemViewModel> entryViewModels)
    {
        List<DefaultEntryItemViewModel> toRemove = entryViewModels.Where(e => !e.ShouldInstall).ToList();
        foreach (DefaultEntryItemViewModel defaultEntryItemViewModel in toRemove)
            entryViewModels.Remove(defaultEntryItemViewModel);

    }

    private async Task GetDefaultEntries(CancellationToken cancellationToken)
    {
        if (!WorkshopReachable)
            return;

        FetchingDefaultEntries = true;

        IOperationResult<IGetDefaultEntriesResult> result = await _client.GetDefaultEntries.ExecuteAsync(100, null, cancellationToken);
        List<IEntrySummary> entries = result.Data?.EntriesV2?.Edges?.Select(e => e.Node).Cast<IEntrySummary>().ToList() ?? [];
        while (result.Data?.EntriesV2?.PageInfo is {HasNextPage: true})
        {
            result = await _client.GetDefaultEntries.ExecuteAsync(100, result.Data.EntriesV2.PageInfo.EndCursor, cancellationToken);
            if (result.Data?.EntriesV2?.Edges != null)
                entries.AddRange(result.Data.EntriesV2.Edges.Select(e => e.Node));
        }

        DeviceProviderEntryViewModels.Clear();
        EssentialEntryViewModels.Clear();
        OtherEntryViewModels.Clear();
        foreach (IEntrySummary entry in entries)
        {
            if (entry.DefaultEntryInfo == null)
                continue;

            DefaultEntryItemViewModel viewModel = _getDefaultEntryItemViewModel(entry);
            viewModel.ShouldInstall = entry.DefaultEntryInfo.IsEssential;
            
            if (entry.DefaultEntryInfo.IsDeviceProvider)
                DeviceProviderEntryViewModels.Add(viewModel);
            else if (entry.DefaultEntryInfo.IsEssential)
                EssentialEntryViewModels.Add(viewModel);
            else
                OtherEntryViewModels.Add(viewModel);
        }

        FetchingDefaultEntries = false;
    }
}