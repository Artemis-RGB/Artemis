using System;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Device;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using ReactiveUI;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.UI.Screens.SurfaceEditor;

public class ListDeviceViewModel : ViewModelBase
{
    private static readonly Random Random = new();
    private readonly IRgbService _rgbService;
    private readonly IWindowService _windowService;

    private SKColor _color;
    private bool _isSelected;

    public ListDeviceViewModel(ArtemisDevice device, SurfaceEditorViewModel surfaceEditorViewModel, IWindowService windowService, IRgbService rgbService)
    {
        _windowService = windowService;
        _rgbService = rgbService;

        Device = device;
        SurfaceEditorViewModel = surfaceEditorViewModel;
        Color = SKColor.FromHsv(Random.NextSingle() * 360, 95, 100);
        DetectInput = ReactiveCommand.CreateFromTask(ExecuteDetectInput, this.WhenAnyValue(vm => vm.CanDetectInput));
    }

    public ReactiveCommand<Unit, Unit> DetectInput { get; }

    public ArtemisDevice Device { get; }
    public SurfaceEditorViewModel SurfaceEditorViewModel { get; }
    public bool CanDetectInput => Device.DeviceType == RGBDeviceType.Keyboard || Device.DeviceType == RGBDeviceType.Mouse;

    public bool IsSelected
    {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public SKColor Color
    {
        get => _color;
        set => RaiseAndSetIfChanged(ref _color, value);
    }

    private async Task ExecuteDetectInput()
    {
        if (!CanDetectInput)
            return;

        await _windowService.CreateContentDialog()
            .WithTitle($"{Device.RgbDevice.DeviceInfo.DeviceName} - Detect input")
            .WithViewModel<DeviceDetectInputViewModel>(out DeviceDetectInputViewModel viewModel, Device)
            .WithCloseButtonText("Cancel")
            .ShowAsync();

        if (viewModel.MadeChanges)
            _rgbService.SaveDevice(Device);
    }
}