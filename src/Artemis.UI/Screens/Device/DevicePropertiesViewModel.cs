using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Shared;
using ReactiveUI;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.UI.Screens.Device;

public class DevicePropertiesViewModel : DialogViewModelBase<object>
{
    private readonly IDeviceVmFactory _deviceVmFactory;
    private ArtemisDevice _device;

    public DevicePropertiesViewModel(ArtemisDevice device, ICoreService coreService, IRgbService rgbService, IDeviceVmFactory deviceVmFactory)
    {
        _deviceVmFactory = deviceVmFactory;
        _device = device;

        SelectedLeds = new ObservableCollection<ArtemisLed>();
        Tabs = new ObservableCollection<ActivatableViewModelBase>();

        AddTabs();
        this.WhenActivated(d =>
        {
            rgbService.DeviceAdded += RgbServiceOnDeviceAdded;
            rgbService.DeviceRemoved += RgbServiceOnDeviceRemoved;
            coreService.FrameRendering += CoreServiceOnFrameRendering;
            Disposable.Create(() => coreService.FrameRendering -= CoreServiceOnFrameRendering).DisposeWith(d);
        });

        ClearSelectedLeds = ReactiveCommand.Create(ExecuteClearSelectedLeds);
    }

    public ArtemisDevice Device
    {
        get => _device;
        set => RaiseAndSetIfChanged(ref _device, value);
    }

    public ObservableCollection<ArtemisLed> SelectedLeds { get; }
    public ObservableCollection<ActivatableViewModelBase> Tabs { get; }
    public ReactiveCommand<Unit, Unit> ClearSelectedLeds { get; }

    private void RgbServiceOnDeviceAdded(object? sender, DeviceEventArgs e)
    {
        if (e.Device.Identifier != Device.Identifier || Device == e.Device)
            return;

        Device = e.Device;
        AddTabs();
    }

    private void RgbServiceOnDeviceRemoved(object? sender, DeviceEventArgs e)
    {
        Tabs.Clear();
        SelectedLeds.Clear();
    }

    private void AddTabs()
    {
        Tabs.Add(_deviceVmFactory.DeviceGeneralTabViewModel(Device));
        Tabs.Add(_deviceVmFactory.DeviceLayoutTabViewModel(Device));
        if (Device.DeviceType == RGBDeviceType.Keyboard)
            Tabs.Add(_deviceVmFactory.InputMappingsTabViewModel(Device, SelectedLeds));
        Tabs.Add(_deviceVmFactory.DeviceLedsTabViewModel(Device, SelectedLeds));
    }

    private void ExecuteClearSelectedLeds()
    {
        SelectedLeds.Clear();
    }

    private void CoreServiceOnFrameRendering(object? sender, FrameRenderingEventArgs e)
    {
        if (!SelectedLeds.Any())
            return;

        using SKPaint highlightPaint = new() {Color = SKColors.White};
        using SKPaint dimPaint = new() {Color = new SKColor(0, 0, 0, 192)};
        foreach (ArtemisLed artemisLed in Device.Leds)
            e.Canvas.DrawRect(artemisLed.AbsoluteRectangle, SelectedLeds.Contains(artemisLed) ? highlightPaint : dimPaint);
    }
}