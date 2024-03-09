using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Device;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using RGB.NET.Core;
using SkiaSharp;
using Point = Avalonia.Point;

namespace Artemis.UI.Screens.SurfaceEditor;

public partial class SurfaceDeviceViewModel : ActivatableViewModelBase
{
    private readonly IDeviceService _deviceService;
    private readonly ISettingsService _settingsService;
    private readonly IWindowService _windowService;
    private double _dragOffsetX;
    private double _dragOffsetY;
    [Notify] private bool _isSelected;
    [Notify] private float _x;
    [Notify] private float _y;

    public SurfaceDeviceViewModel(ArtemisDevice device, SurfaceEditorViewModel surfaceEditorViewModel, IDeviceService deviceService, ISettingsService settingsService, IWindowService windowService)
    {
        _deviceService = deviceService;
        _settingsService = settingsService;
        _windowService = windowService;

        Device = device;
        SurfaceEditorViewModel = surfaceEditorViewModel;
        DetectInput = ReactiveCommand.CreateFromTask(ExecuteDetectInput, this.WhenAnyValue(vm => vm.CanDetectInput));
        X = device.X;
        Y = device.Y;
        
        this.WhenActivated(d =>
        {
            Device.PropertyChanged += DeviceOnPropertyChanged;
            Disposable.Create(() => Device.PropertyChanged -= DeviceOnPropertyChanged).DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit> DetectInput { get; }
    public ArtemisDevice Device { get; }
    public SurfaceEditorViewModel SurfaceEditorViewModel { get; }
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
            X = x;
            Y = y;
        }
        else if (Fits(x, Device.Y, ignoreOverlap))
        {
            X = x;
        }
        else if (Fits(Device.X, y, ignoreOverlap))
        {
            Y = y;
        }
    }

    private bool Fits(float x, float y, bool ignoreOverlap)
    {
        if (x < 0 || y < 0)
            return false;

        double maxTextureSize = 4096 / _settingsService.GetSetting("Core.RenderScale", 0.5).Value;
        if (x + Device.Rectangle.Width > maxTextureSize || y + Device.Rectangle.Height > maxTextureSize)
            return false;

        if (ignoreOverlap)
            return true;

        IEnumerable<SKRect> own = Device.Leds
            .Select(l => SKRect.Create(l.Rectangle.Left + x, l.Rectangle.Top + y, l.Rectangle.Width, l.Rectangle.Height));
        IEnumerable<SKRect> others = SurfaceEditorViewModel.SurfaceDeviceViewModels
            .Where(vm => vm != this && vm.Device.IsEnabled)
            .SelectMany(vm => vm.Device.Leds.Select(l => SKRect.Create(l.Rectangle.Left + vm.X, l.Rectangle.Top + vm.Y, l.Rectangle.Width, l.Rectangle.Height)));

        return !own.Any(o => others.Any(l => l.IntersectsWith(o)));
    }

    private async Task ExecuteDetectInput()
    {
        if (!CanDetectInput)
            return;

        await _windowService.CreateContentDialog()
            .WithTitle($"{Device.RgbDevice.DeviceInfo.DeviceName} - Detect input")
            .WithViewModel(out DeviceDetectInputViewModel viewModel, Device)
            .WithCloseButtonText("Cancel")
            .ShowAsync();

        if (viewModel.MadeChanges)
            _deviceService.SaveDevice(Device);
    }

    public void Apply()
    {
        Device.X = X;
        Device.Y = Y;
    }
    
    private void DeviceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Device.X))
            X = Device.X;
        if (e.PropertyName == nameof(Device.Y))
            Y = Device.Y;
    }
}