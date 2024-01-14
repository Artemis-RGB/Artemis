using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Providers;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Providers;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Device.Layout.LayoutProviders;

public partial class WorkshopLayoutViewModel : ActivatableViewModelBase, ILayoutProviderViewModel
{
    [Notify] private InstalledEntry? _selectedEntry;
    private readonly WorkshopLayoutProvider _layoutProvider;
    private readonly IDeviceService _deviceService;

    public WorkshopLayoutViewModel(WorkshopLayoutProvider layoutProvider, IWorkshopService workshopService, IDeviceService deviceService)
    {
        _layoutProvider = layoutProvider;
        _deviceService = deviceService;
        Entries = new ObservableCollection<InstalledEntry>(workshopService.GetInstalledEntries().Where(e => e.EntryType == EntryType.Layout));

        this.WhenAnyValue(vm => vm.SelectedEntry).Subscribe(ApplyEntry);
        this.WhenActivated((CompositeDisposable _) => SelectedEntry = Entries.FirstOrDefault(e => e.EntryId.ToString() == Device.LayoutSelection.Parameter));
    }

    /// <inheritdoc />
    public ILayoutProvider Provider => _layoutProvider;

    public ArtemisDevice Device { get; set; } = null!;

    public ObservableCollection<InstalledEntry> Entries { get; }

    /// <inheritdoc />
    public string Name => "Workshop";

    /// <inheritdoc />
    public string Description => "Load a layout from the workshop";

    /// <inheritdoc />
    public void Apply()
    {
        _layoutProvider.ConfigureDevice(Device, null);
        Save();
    }

    private void ApplyEntry(InstalledEntry? entry)
    {
        if (entry == null || Device.LayoutSelection.Parameter == entry.EntryId.ToString())
            return;
        _layoutProvider.ConfigureDevice(Device, entry);
        Save();
    }

    private void Save()
    {
        _deviceService.SaveDevice(Device);
        _deviceService.LoadDeviceLayout(Device);
    }
}