using System;
using System.Reactive;
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

namespace Artemis.UI.Screens.SurfaceEditor;

public partial class ListDeviceViewModel : ViewModelBase
{
    private static readonly Random Random = new();
    private readonly IWindowService _windowService;
    private readonly IDeviceService _deviceService;
    [Notify] private SKColor _color;
    [Notify] private bool _isSelected;

    public ListDeviceViewModel(ArtemisDevice device, IWindowService windowService, IDeviceService deviceService)
    {
        _windowService = windowService;
        _deviceService = deviceService;

        Device = device;
        Color = SKColor.FromHsv(Random.NextSingle() * 360, 95, 100);
        DetectInput = ReactiveCommand.CreateFromTask(ExecuteDetectInput, this.WhenAnyValue(vm => vm.CanDetectInput));
    }

    public ReactiveCommand<Unit, Unit> DetectInput { get; }

    public ArtemisDevice Device { get; }
    public bool CanDetectInput => Device.DeviceType == RGBDeviceType.Keyboard || Device.DeviceType == RGBDeviceType.Mouse;
    
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
}