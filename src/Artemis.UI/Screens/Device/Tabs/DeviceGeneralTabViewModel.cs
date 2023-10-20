using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.UI.Screens.Device;

public partial class DeviceGeneralTabViewModel : ActivatableViewModelBase
{
    private readonly IDeviceService _deviceService;
    private readonly IRenderService _renderService;
    private readonly IWindowService _windowService;
    private readonly List<DeviceCategory> _categories;
    private readonly float _initialBlueScale;
    private readonly float _initialGreenScale;
    private readonly float _initialRedScale;

    [Notify] private int _rotation;
    [Notify] private float _scale;
    [Notify] private int _x;
    [Notify] private int _y;
    [Notify] private float _redScale;
    [Notify] private float _greenScale;
    [Notify] private float _blueScale;
    [Notify] private SKColor _currentColor;
    [Notify] private bool _displayOnDevices;

    public DeviceGeneralTabViewModel(ArtemisDevice device, IDeviceService deviceService, IRenderService renderService, IWindowService windowService)
    {
        _deviceService = deviceService;
        _renderService = renderService;
        _windowService = windowService;
        _categories = new List<DeviceCategory>(device.Categories);

        Device = device;
        DisplayName = "General";
        X = (int) Device.X;
        Y = (int) Device.Y;
        Scale = Device.Scale;
        Rotation = (int) Device.Rotation;
        RedScale = Device.RedScale * 100f;
        GreenScale = Device.GreenScale * 100f;
        BlueScale = Device.BlueScale * 100f;
        CurrentColor = SKColors.White;

        // We need to store the initial values to be able to restore them when the user clicks "Cancel"
        _initialRedScale = Device.RedScale;
        _initialGreenScale = Device.GreenScale;
        _initialBlueScale = Device.BlueScale;

        this.WhenAnyValue(x => x.RedScale, x => x.GreenScale, x => x.BlueScale).Subscribe(_ => ApplyScaling());

        this.WhenActivated(d =>
        {
            _renderService.FrameRendering += OnFrameRendering;

            Disposable.Create(() =>
            {
                _renderService.FrameRendering -= OnFrameRendering;
                Apply();
            }).DisposeWith(d);
        });
    }

    public bool RequiresManualSetup => !Device.DeviceProvider.CanDetectPhysicalLayout || !Device.DeviceProvider.CanDetectLogicalLayout;

    public ArtemisDevice Device { get; }
    
    public bool IsKeyboard => Device.DeviceType == RGBDeviceType.Keyboard;

    public bool HasDeskCategory
    {
        get => GetCategory(DeviceCategory.Desk);
        set => SetCategory(DeviceCategory.Desk, value);
    }

    public bool HasMonitorCategory
    {
        get => GetCategory(DeviceCategory.Monitor);
        set => SetCategory(DeviceCategory.Monitor, value);
    }

    public bool HasCaseCategory
    {
        get => GetCategory(DeviceCategory.Case);
        set => SetCategory(DeviceCategory.Case, value);
    }

    public bool HasRoomCategory
    {
        get => GetCategory(DeviceCategory.Room);
        set => SetCategory(DeviceCategory.Room, value);
    }

    public bool HasPeripheralsCategory
    {
        get => GetCategory(DeviceCategory.Peripherals);
        set => SetCategory(DeviceCategory.Peripherals, value);
    }
    
    private bool GetCategory(DeviceCategory category)
    {
        return _categories.Contains(category);
    }

    private void SetCategory(DeviceCategory category, bool value)
    {
        if (value && !_categories.Contains(category))
            _categories.Add(category);
        else if (!value)
            _categories.Remove(category);

        this.RaisePropertyChanged($"Has{category}Category");
    }

    public async Task RestartSetup()
    {
        if (!RequiresManualSetup)
            return;
        if (!Device.DeviceProvider.CanDetectPhysicalLayout && !await DevicePhysicalLayoutDialogViewModel.SelectPhysicalLayout(_windowService, Device))
            return;
        if (!Device.DeviceProvider.CanDetectLogicalLayout && !await DeviceLogicalLayoutDialogViewModel.SelectLogicalLayout(_windowService, Device))
            return;

        await Task.Delay(400);
        _deviceService.SaveDevice(Device);
        _deviceService.ApplyDeviceLayout(Device, Device.GetBestDeviceLayout());
    }

    private void Apply()
    {
        Device.X = X;
        Device.Y = Y;
        Device.Scale = Scale;
        Device.Rotation = Rotation;
        Device.RedScale = RedScale / 100f;
        Device.GreenScale = GreenScale / 100f;
        Device.BlueScale = BlueScale / 100f;
        Device.Categories.Clear();
        foreach (DeviceCategory deviceCategory in _categories)
            Device.Categories.Add(deviceCategory);

        _deviceService.SaveDevice(Device);
    }

    public void ApplyScaling()
    {
        Device.RedScale = RedScale / 100f;
        Device.GreenScale = GreenScale / 100f;
        Device.BlueScale = BlueScale / 100f;
        Device.RgbDevice.Update(true);
    }

    public void ResetScaling()
    {
        RedScale = _initialRedScale * 100;
        GreenScale = _initialGreenScale * 100;
        BlueScale = _initialBlueScale * 100;
        Device.RgbDevice.Update(true);
    }

    private void OnFrameRendering(object? sender, FrameRenderingEventArgs e)
    {
        if (!DisplayOnDevices)
            return;

        using SKPaint overlayPaint = new() {Color = CurrentColor};
        e.Canvas.DrawRect(0, 0, e.Canvas.LocalClipBounds.Width, e.Canvas.LocalClipBounds.Height, overlayPaint);
    }
}