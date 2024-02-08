using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Providers;
using Artemis.WebClient.Workshop.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Layout.Dialogs;

public class DeviceSelectionDialogViewModel : ContentDialogViewModelBase
{
    private readonly IDeviceService _deviceService;
    private readonly WorkshopLayoutProvider _layoutProvider;

    public DeviceSelectionDialogViewModel(List<ArtemisDevice> devices, InstalledEntry entry, ISurfaceVmFactory surfaceVmFactory, IDeviceService deviceService, WorkshopLayoutProvider layoutProvider)
    {
        _deviceService = deviceService;
        _layoutProvider = layoutProvider;
        Entry = entry;
        Devices = new ObservableCollection<ListDeviceViewModel>(devices.Select(surfaceVmFactory.ListDeviceViewModel));
        Apply = ReactiveCommand.Create(ExecuteApply);
    }

    public InstalledEntry Entry { get; }
    public ObservableCollection<ListDeviceViewModel> Devices { get; }
    public ReactiveCommand<Unit, Unit> Apply { get; }

    private void ExecuteApply()
    {
        foreach (ListDeviceViewModel listDeviceViewModel in Devices.Where(d => d.IsSelected))
        {
            _layoutProvider.ConfigureDevice(listDeviceViewModel.Device, Entry);
            _deviceService.SaveDevice(listDeviceViewModel.Device);
            _deviceService.LoadDeviceLayout(listDeviceViewModel.Device);
        }
    }
}