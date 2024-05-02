using System;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Handlers.InstallationHandlers;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Providers;
using Artemis.WebClient.Workshop.Services;
using Material.Icons;
using Material.Icons.Avalonia;
using PropertyChanged.SourceGenerator;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.LayoutFinder;

public partial class LayoutFinderDeviceViewModel : ViewModelBase
{
    private readonly IWorkshopClient _client;
    private readonly IDeviceService _deviceService;
    private readonly IWorkshopService _workshopService;
    private readonly WorkshopLayoutProvider _layoutProvider;
    private readonly EntryInstallationHandlerFactory _factory;

    [Notify] private bool _searching;
    [Notify] private bool _hasLayout;

    [Notify] private IEntrySummary? _entry;
    [Notify] private IRelease? _release;
    [Notify] private string? _logicalLayout;
    [Notify] private string? _physicalLayout;

    public LayoutFinderDeviceViewModel(ArtemisDevice device,
        IWorkshopClient client,
        IDeviceService deviceService,
        IWorkshopService workshopService,
        WorkshopLayoutProvider layoutProvider,
        EntryInstallationHandlerFactory factory)
    {
        _client = client;
        _deviceService = deviceService;
        _workshopService = workshopService;
        _layoutProvider = layoutProvider;
        _factory = factory;

        Device = device;
        DeviceIcon = DetermineDeviceIcon();
        HasLayout = Device.Layout != null && !Device.Layout.IsDefaultLayout;
    }

    public ArtemisDevice Device { get; }
    public MaterialIconKind DeviceIcon { get; }

    public async Task Search()
    {
        if (HasLayout)
            return;
        
        try
        {
            Searching = true;
            Task delayTask = Task.Delay(400);

            if (Device.DeviceType == RGB.NET.Core.RGBDeviceType.Keyboard)
                await SearchKeyboardLayout();
            else
                await SearchLayout();

            if (Entry != null && Release != null)
                await InstallAndApplyEntry(Entry, Release);

            await delayTask;
        }
        finally
        {
            Searching = false;
            HasLayout = Device.Layout != null && !Device.Layout.IsDefaultLayout;
        }
    }

    private async Task SearchKeyboardLayout()
    {
        IOperationResult<ISearchKeyboardLayoutResult> result = await _client.SearchKeyboardLayout.ExecuteAsync(
            Device.DeviceProvider.Plugin.Guid,
            Device.RgbDevice.DeviceInfo.Model,
            Device.RgbDevice.DeviceInfo.Manufacturer,
            Device.LogicalLayout,
            Enum.Parse<Artemis.WebClient.Workshop.KeyboardLayoutType>(Device.PhysicalLayout.ToString(), true));

        Entry = result.Data?.SearchKeyboardLayout?.Entry;
        Release = result.Data?.SearchKeyboardLayout?.Entry.LatestRelease;
        LogicalLayout = result.Data?.SearchKeyboardLayout?.LogicalLayout;
        PhysicalLayout = result.Data?.SearchKeyboardLayout?.PhysicalLayout.ToString();
    }

    private async Task SearchLayout()
    {
        IOperationResult<ISearchLayoutResult> result = await _client.SearchLayout.ExecuteAsync(
            Enum.Parse<RGBDeviceType>(Device.DeviceType.ToString(), true),
            Device.DeviceProvider.Plugin.Guid,
            Device.RgbDevice.DeviceInfo.Model,
            Device.RgbDevice.DeviceInfo.Manufacturer);

        Entry = result.Data?.SearchLayout?.Entry;
        Release = result.Data?.SearchLayout?.Entry.LatestRelease;
        LogicalLayout = null;
        PhysicalLayout = null;
    }

    private async Task InstallAndApplyEntry(IEntrySummary entry, IRelease release)
    {
        // Try a local install first
        InstalledEntry? installedEntry = _workshopService.GetInstalledEntry(entry.Id);
        if (installedEntry == null)
        {
            IEntryInstallationHandler installationHandler = _factory.CreateHandler(EntryType.Layout);
            EntryInstallResult result = await installationHandler.InstallAsync(entry, release, new Progress<StreamProgress>(), CancellationToken.None);
            installedEntry = result.Entry;
        }

        if (installedEntry != null)
        {
            _layoutProvider.ConfigureDevice(Device, installedEntry);
            _deviceService.SaveDevice(Device);
            _deviceService.LoadDeviceLayout(Device);
        }
    }

    private MaterialIconKind DetermineDeviceIcon()
    {
        return Device.DeviceType switch
        {
            RGB.NET.Core.RGBDeviceType.None => MaterialIconKind.QuestionMarkCircle,
            RGB.NET.Core.RGBDeviceType.Keyboard => MaterialIconKind.Keyboard,
            RGB.NET.Core.RGBDeviceType.Mouse => MaterialIconKind.Mouse,
            RGB.NET.Core.RGBDeviceType.Headset => MaterialIconKind.Headset,
            RGB.NET.Core.RGBDeviceType.Mousepad => MaterialIconKind.TextureBox,
            RGB.NET.Core.RGBDeviceType.LedStripe => MaterialIconKind.LightStrip,
            RGB.NET.Core.RGBDeviceType.LedMatrix => MaterialIconKind.DrawingBox,
            RGB.NET.Core.RGBDeviceType.Mainboard => MaterialIconKind.Chip,
            RGB.NET.Core.RGBDeviceType.GraphicsCard => MaterialIconKind.GraphicsProcessingUnit,
            RGB.NET.Core.RGBDeviceType.DRAM => MaterialIconKind.Memory,
            RGB.NET.Core.RGBDeviceType.HeadsetStand => MaterialIconKind.HeadsetDock,
            RGB.NET.Core.RGBDeviceType.Keypad => MaterialIconKind.Keypad,
            RGB.NET.Core.RGBDeviceType.Fan => MaterialIconKind.Fan,
            RGB.NET.Core.RGBDeviceType.Speaker => MaterialIconKind.Speaker,
            RGB.NET.Core.RGBDeviceType.Cooler => MaterialIconKind.FreezingPoint,
            RGB.NET.Core.RGBDeviceType.Monitor => MaterialIconKind.DesktopWindows,
            RGB.NET.Core.RGBDeviceType.LedController => MaterialIconKind.LedStripVariant,
            RGB.NET.Core.RGBDeviceType.GameController => MaterialIconKind.MicrosoftXboxController,
            RGB.NET.Core.RGBDeviceType.Unknown => MaterialIconKind.QuestionMarkCircle,
            RGB.NET.Core.RGBDeviceType.All => MaterialIconKind.QuestionMarkCircle,
            _ => MaterialIconKind.QuestionMarkCircle
        };
    }
}