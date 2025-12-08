using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
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
    [Notify] private bool _fetchingDefaultEntries = true;
    [Notify] private int _installedEntries;
    [Notify] private int _totalEntries = 1;
    [Notify] private DefaultEntryItemViewModel? _currentEntry;

    private readonly IDeviceService _deviceService;
    private readonly IWorkshopClient _client;
    private readonly Func<IEntrySummary, DefaultEntryItemViewModel> _getDefaultEntryItemViewModel;
    private readonly ObservableAsPropertyHelper<int> _remainingEntries;

    public ObservableCollection<DefaultEntryItemViewModel> DeviceProviderEntryViewModels { get; } = [];
    public ObservableCollection<DefaultEntryItemViewModel> EssentialEntryViewModels { get; } = [];
    public ObservableCollection<DefaultEntryItemViewModel> OtherEntryViewModels { get; } = [];
    public int RemainingEntries => _remainingEntries.Value;
    
    public DefaultEntriesStepViewModel(IWorkshopService workshopService, IDeviceService deviceService, IWorkshopClient client,
        Func<IEntrySummary, DefaultEntryItemViewModel> getDefaultEntryItemViewModel)
    {
        _deviceService = deviceService;
        _client = client;
        _getDefaultEntryItemViewModel = getDefaultEntryItemViewModel;
        _remainingEntries = this.WhenAnyValue(vm => vm.InstalledEntries, vm => vm.TotalEntries)
            .Select(t => t.Item2 - t.Item1 + 1)
            .ToProperty(this, vm => vm.RemainingEntries);

        ContinueText = "Install selected entries";
        Continue = ReactiveCommand.CreateFromTask(async ct =>
        { 
            await Install(ct);
            ExecuteContinue();
        }, this.WhenAnyValue(vm => vm.FetchingDefaultEntries).Select(b => !b));
        GoBack = ReactiveCommand.Create(() => Wizard.ChangeScreen<WelcomeStepViewModel>());

        this.WhenActivatedAsync(async d =>
        {
            CancellationToken ct = d.AsCancellationToken();
            if (await workshopService.ValidateWorkshopStatus(false, ct))
                await GetDefaultEntries(d.AsCancellationToken());
            else if (!ct.IsCancellationRequested)
                Wizard.ChangeScreen<WorkshopUnreachableStepViewModel>();
        });
    }

    private void ExecuteContinue()
    {
        // Without devices skip to the last step
        if (_deviceService.EnabledDevices.Count == 0)
            Wizard.ChangeScreen<SettingsStepViewModel>();
        else
            Wizard.ChangeScreen<LayoutsStepViewModel>();
    }

    private async Task Install(CancellationToken cancellationToken)
    {
        List<DefaultEntryItemViewModel> entries =
        [
            ..DeviceProviderEntryViewModels.Where(e => e.ShouldInstall && !e.IsInstalled),
            ..EssentialEntryViewModels.Where(e => e.ShouldInstall && !e.IsInstalled),
            ..OtherEntryViewModels.Where(e => e.ShouldInstall && !e.IsInstalled)
        ];
        InstalledEntries = 0;
        TotalEntries = entries.Count;

        // Continue to the next screen if there are no entries to install
        if (TotalEntries == 0)
            return;

        foreach (DefaultEntryItemViewModel defaultEntryItemViewModel in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CurrentEntry = defaultEntryItemViewModel;
            await defaultEntryItemViewModel.InstallEntry(cancellationToken);
            InstalledEntries++;
        }

        await Task.Delay(1000, cancellationToken);
        CurrentEntry = null;
    }

    private async Task GetDefaultEntries(CancellationToken cancellationToken)
    {
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