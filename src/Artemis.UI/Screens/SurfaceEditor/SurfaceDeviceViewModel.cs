using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using ReactiveUI;
using RGB.NET.Core;
using SkiaSharp;
using Point = Avalonia.Point;

namespace Artemis.UI.Screens.SurfaceEditor;

public class SurfaceDeviceViewModel : ActivatableViewModelBase
{
    private readonly IDeviceService _deviceService;
    private readonly IDeviceVmFactory _deviceVmFactory;
    private readonly IRgbService _rgbService;
    private readonly ISettingsService _settingsService;
    private readonly IWindowService _windowService;
    private double _dragOffsetX;
    private double _dragOffsetY;
    private bool _isSelected;

    public SurfaceDeviceViewModel(ArtemisDevice device, SurfaceEditorViewModel surfaceEditorViewModel, IRgbService rgbService, IDeviceService deviceService, ISettingsService settingsService,
        IDeviceVmFactory deviceVmFactory,
        IWindowService windowService)
    {
        _rgbService = rgbService;
        _deviceService = deviceService;
        _settingsService = settingsService;
        _deviceVmFactory = deviceVmFactory;
        _windowService = windowService;

        Device = device;
        SurfaceEditorViewModel = surfaceEditorViewModel;

        IdentifyDevice = ReactiveCommand.Create<ArtemisDevice>(ExecuteIdentifyDevice);
        ViewProperties = ReactiveCommand.CreateFromTask<ArtemisDevice>(ExecuteViewProperties);
    }

    public ReactiveCommand<ArtemisDevice, Unit> IdentifyDevice { get; }
    public ReactiveCommand<ArtemisDevice, Unit> ViewProperties { get; }

    public ArtemisDevice Device { get; }
    public SurfaceEditorViewModel SurfaceEditorViewModel { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public bool CanDetectInput => Device.DeviceType == RGBDeviceType.Keyboard || Device.DeviceType == RGBDeviceType.Mouse;

    public void StartMouseDrag(Point mouseStartPosition)
    {
        if (!IsSelected)
            return;

        _dragOffsetX = Device.X - mouseStartPosition.X;
        _dragOffsetY = Device.Y - mouseStartPosition.Y;
    }

    public void UpdateMouseDrag(Point mousePosition, bool round, bool ignoreOverlap)
    {
        if (!IsSelected)
            return;

        float x = (float) (mousePosition.X + _dragOffsetX);
        float y = (float) (mousePosition.Y + _dragOffsetY);

        if (round)
        {
            x = (float) Math.Round(x / 10d, 0, MidpointRounding.AwayFromZero) * 10f;
            y = (float) Math.Round(y / 10d, 0, MidpointRounding.AwayFromZero) * 10f;
        }


        if (Fits(x, y, ignoreOverlap))
        {
            Device.X = x;
            Device.Y = y;
        }
        else if (Fits(x, Device.Y, ignoreOverlap))
        {
            Device.X = x;
        }
        else if (Fits(Device.X, y, ignoreOverlap))
        {
            Device.Y = y;
        }
    }

    private void ExecuteIdentifyDevice(ArtemisDevice device)
    {
        _deviceService.IdentifyDevice(device);
    }

    private async Task ExecuteViewProperties(ArtemisDevice device)
    {
        await _windowService.ShowDialogAsync(_deviceVmFactory.DevicePropertiesViewModel(device));
    }

    private bool Fits(float x, float y, bool ignoreOverlap)
    {
        if (x < 0 || y < 0)
            return false;

        double maxTextureSize = 4096 / _settingsService.GetSetting("Core.RenderScale", 0.25).Value;
        if (x + Device.Rectangle.Width > maxTextureSize || y + Device.Rectangle.Height > maxTextureSize)
            return false;

        if (ignoreOverlap)
            return true;

        IEnumerable<SKRect> own = Device.Leds
            .Select(l => SKRect.Create(l.Rectangle.Left + x, l.Rectangle.Top + y, l.Rectangle.Width, l.Rectangle.Height));
        IEnumerable<SKRect> others = _rgbService.EnabledDevices
            .Where(d => d != Device && d.IsEnabled)
            .SelectMany(d => d.Leds)
            .Select(l => SKRect.Create(l.Rectangle.Left + l.Device.X, l.Rectangle.Top + l.Device.Y, l.Rectangle.Width, l.Rectangle.Height));

        return !own.Any(o => others.Any(l => l.IntersectsWith(o)));
    }
}