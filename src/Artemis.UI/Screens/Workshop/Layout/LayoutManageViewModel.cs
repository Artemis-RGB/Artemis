using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Providers;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Threading;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutManageViewModel : RoutableScreen<WorkshopDetailParameters>
{
    private readonly ISurfaceVmFactory _surfaceVmFactory;
    private readonly IRouter _router;
    private readonly IWorkshopService _workshopService;
    private readonly IDeviceService _deviceService;
    private readonly WorkshopLayoutProvider _layoutProvider;
    private readonly IWindowService _windowService;
    [Notify] private ArtemisLayout? _layout;
    [Notify] private InstalledEntry? _entry;
    [Notify] private ObservableCollection<ListDeviceViewModel>? _devices;

    public LayoutManageViewModel(ISurfaceVmFactory surfaceVmFactory,
        IRouter router,
        IWorkshopService workshopService,
        IDeviceService deviceService,
        WorkshopLayoutProvider layoutProvider,
        IWindowService windowService)
    {
        _surfaceVmFactory = surfaceVmFactory;
        _router = router;
        _workshopService = workshopService;
        _deviceService = deviceService;
        _layoutProvider = layoutProvider;
        _windowService = windowService;
        Apply = ReactiveCommand.Create(ExecuteApply);
        ParameterSource = ParameterSource.Route;
    }

    public ReactiveCommand<Unit, Unit> Apply { get; }

    public async Task Close()
    {
        await _router.GoUp();
    }

    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        InstalledEntry? installedEntry = _workshopService.GetInstalledEntry(parameters.EntryId);
        if (installedEntry == null)
        {
            // TODO: Fix cancelling without this workaround, currently navigation is stopped but the page still opens
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await _windowService.ShowConfirmContentDialog("Entry not found", "The entry you're trying to manage could not be found.", "Go back", null);
                await Close();
            });
            return;
        }

        Layout = new ArtemisLayout(Path.Combine(installedEntry.GetReleaseDirectory().FullName, "layout.xml"));
        if (!Layout.IsValid)
        {
            // TODO: Fix cancelling without this workaround, currently navigation is stopped but the page still opens
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await _windowService.ShowConfirmContentDialog("Invalid layout", "The layout of the entry you're trying to manage is invalid.", "Go back", null);
                await Close();
            });
            return;
        }

        Entry = installedEntry;
        Devices = new ObservableCollection<ListDeviceViewModel>(_deviceService.Devices
            .Where(d => d.RgbDevice.DeviceInfo.DeviceType == Layout.RgbLayout.Type)
            .Select(_surfaceVmFactory.ListDeviceViewModel));
    }

    private void ExecuteApply()
    {
        if (Devices == null)
            return;

        foreach (ListDeviceViewModel listDeviceViewModel in Devices.Where(d => d.IsSelected))
        {
            _layoutProvider.ConfigureDevice(listDeviceViewModel.Device, Entry);
            _deviceService.SaveDevice(listDeviceViewModel.Device);
            _deviceService.LoadDeviceLayout(listDeviceViewModel.Device);
        }
    }
}