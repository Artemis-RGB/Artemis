using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Extensions;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Avalonia;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.SurfaceEditor;

public class SurfaceEditorViewModel : RoutableScreen, IMainScreenViewModel
{
    private readonly IDeviceService _deviceService;
    private readonly IRenderService _renderService;
    private readonly IDeviceVmFactory _deviceVmFactory;
    private readonly ISettingsService _settingsService;
    private readonly ISurfaceVmFactory _surfaceVmFactory;
    private readonly IWindowService _windowService;
    private bool _colorDevices;
    private bool _colorFirstLedOnly;
    private List<SurfaceDeviceViewModel>? _initialSelection;
    private double _overlayOpacity;
    private bool _saving;

    public SurfaceEditorViewModel(ICoreService coreService,
        ISurfaceVmFactory surfaceVmFactory,
        ISettingsService settingsService,
        IDeviceVmFactory deviceVmFactory,
        IWindowService windowService,
        IDeviceService deviceService,
        IRenderService renderService)
    {
        _surfaceVmFactory = surfaceVmFactory;
        _settingsService = settingsService;
        _deviceVmFactory = deviceVmFactory;
        _windowService = windowService;
        _deviceService = deviceService;
        _renderService = renderService;

        DisplayName = "Surface Editor";
        SurfaceDeviceViewModels = new ObservableCollection<SurfaceDeviceViewModel>(deviceService.EnabledDevices.OrderBy(d => d.ZIndex).Select(d => surfaceVmFactory.SurfaceDeviceViewModel(d, this)));
        ListDeviceViewModels = new ObservableCollection<ListDeviceViewModel>(deviceService.EnabledDevices.OrderBy(d => d.ZIndex).Select(d => surfaceVmFactory.ListDeviceViewModel(d, this)));

        AutoArrange = ReactiveCommand.CreateFromTask(ExecuteAutoArrange);
        IdentifyDevice = ReactiveCommand.Create<ArtemisDevice>(ExecuteIdentifyDevice);
        ViewProperties = ReactiveCommand.CreateFromTask<ArtemisDevice>(ExecuteViewProperties);
        BringToFront = ReactiveCommand.Create<ArtemisDevice>(ExecuteBringToFront);
        BringForward = ReactiveCommand.Create<ArtemisDevice>(ExecuteBringForward);
        SendToBack = ReactiveCommand.Create<ArtemisDevice>(ExecuteSendToBack);
        SendBackward = ReactiveCommand.Create<ArtemisDevice>(ExecuteSendBackward);

        this.WhenActivated(d =>
        {
            _renderService.FrameRendering += RenderServiceOnFrameRendering;
            _deviceService.DeviceAdded += DeviceServiceOnDeviceAdded;
            _deviceService.DeviceRemoved += DeviceServiceOnDeviceRemoved;
            Disposable.Create(() =>
            {
                _renderService.FrameRendering -= RenderServiceOnFrameRendering;
                _deviceService.DeviceAdded -= DeviceServiceOnDeviceAdded;
                _deviceService.DeviceRemoved -= DeviceServiceOnDeviceRemoved;
            }).DisposeWith(d);
        });
    }
    
    public ViewModelBase? TitleBarViewModel => null;

    public bool ColorDevices
    {
        get => _colorDevices;
        set => RaiseAndSetIfChanged(ref _colorDevices, value);
    }

    public bool ColorFirstLedOnly
    {
        get => _colorFirstLedOnly;
        set => RaiseAndSetIfChanged(ref _colorFirstLedOnly, value);
    }

    public ObservableCollection<SurfaceDeviceViewModel> SurfaceDeviceViewModels { get; }
    public ObservableCollection<ListDeviceViewModel> ListDeviceViewModels { get; }

    public ReactiveCommand<Unit, Unit> AutoArrange { get; }
    public ReactiveCommand<ArtemisDevice, Unit> IdentifyDevice { get; }
    public ReactiveCommand<ArtemisDevice, Unit> ViewProperties { get; }
    public ReactiveCommand<ArtemisDevice, Unit> BringToFront { get; }
    public ReactiveCommand<ArtemisDevice, Unit> BringForward { get; }
    public ReactiveCommand<ArtemisDevice, Unit> SendToBack { get; }
    public ReactiveCommand<ArtemisDevice, Unit> SendBackward { get; }

    public double MaxTextureSize => 4096 / _settingsService.GetSetting("Core.RenderScale", 0.25).Value;

    public void UpdateSelection(List<SurfaceDeviceViewModel> devices, bool expand, bool invert)
    {
        _initialSelection ??= SurfaceDeviceViewModels.Where(d => d.IsSelected).ToList();

        if (expand)
        {
            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in devices)
                surfaceDeviceViewModel.IsSelected = true;
        }
        else if (invert)
        {
            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in devices)
                surfaceDeviceViewModel.IsSelected = !_initialSelection.Contains(surfaceDeviceViewModel);
        }
        else
        {
            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in devices)
                surfaceDeviceViewModel.IsSelected = true;
            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels.Except(devices))
                surfaceDeviceViewModel.IsSelected = false;
        }
    }

    public void FinishSelection()
    {
        _initialSelection = null;

        if (_saving)
            return;

        Task.Run(() =>
        {
            try
            {
                _saving = true;
                foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels)
                    surfaceDeviceViewModel.Apply();
                _deviceService.SaveDevices();
            }
            catch (Exception e)
            {
                _windowService.ShowExceptionDialog("Failed to update device positions", e);
            }
            finally
            {
                _saving = false;
            }
        });
    }

    public void ClearSelection()
    {
        foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels)
            surfaceDeviceViewModel.IsSelected = false;
    }

    public void StartMouseDrag(Point mousePosition)
    {
        foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels)
            surfaceDeviceViewModel.StartMouseDrag(mousePosition);
    }

    public void UpdateMouseDrag(Point mousePosition, bool round, bool ignoreOverlap)
    {
        foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels)
            surfaceDeviceViewModel.UpdateMouseDrag(mousePosition, round, ignoreOverlap);
    }

    private void DeviceServiceOnDeviceAdded(object? sender, DeviceEventArgs e)
    {
        if (!e.Device.IsEnabled)
            return;

        SurfaceDeviceViewModels.Add(_surfaceVmFactory.SurfaceDeviceViewModel(e.Device, this));
        ListDeviceViewModels.Add(_surfaceVmFactory.ListDeviceViewModel(e.Device, this));
        SurfaceDeviceViewModels.Sort(l => l.Device.ZIndex * -1);
        ListDeviceViewModels.Sort(l => l.Device.ZIndex * -1);
    }

    private void DeviceServiceOnDeviceRemoved(object? sender, DeviceEventArgs e)
    {
        SurfaceDeviceViewModel? surfaceVm = SurfaceDeviceViewModels.FirstOrDefault(vm => vm.Device == e.Device);
        ListDeviceViewModel? listVm = ListDeviceViewModels.FirstOrDefault(vm => vm.Device == e.Device);
        if (surfaceVm != null)
            SurfaceDeviceViewModels.Remove(surfaceVm);
        if (listVm != null)
            ListDeviceViewModels.Remove(listVm);
    }

    private async Task ExecuteAutoArrange()
    {
        bool confirmed = await _windowService.ShowConfirmContentDialog("Auto-arrange layout", "Are you sure you want to auto-arrange your layout? Your current settings will be overwritten.");
        if (!confirmed)
            return;

        _deviceService.AutoArrangeDevices();
    }

    private void RenderServiceOnFrameRendering(object? sender, FrameRenderingEventArgs e)
    {
        // Animate the overlay because I'm vain
        if (ColorDevices && _overlayOpacity < 1)
            _overlayOpacity = Math.Min(1, _overlayOpacity + e.DeltaTime * 3);
        else if (!ColorDevices && _overlayOpacity > 0)
            _overlayOpacity = Math.Max(0, _overlayOpacity - e.DeltaTime * 3);

        if (_overlayOpacity == 0)
            return;

        using SKPaint paint = new();
        byte alpha = (byte) (Easings.CubicEaseInOut(_overlayOpacity) * 255);

        // Fill the entire canvas with a black backdrop
        paint.Color = SKColors.Black.WithAlpha(alpha);
        e.Canvas.DrawRect(e.Canvas.LocalClipBounds, paint);

        // Draw a rectangle for each LED
        foreach (ListDeviceViewModel listDeviceViewModel in ListDeviceViewModels)
        {
            // Order by position to accurately get the first LED
            List<ArtemisLed> leds = listDeviceViewModel.Device.Leds.OrderBy(l => l.RgbLed.Location.Y).ThenBy(l => l.RgbLed.Location.X).ToList();
            for (int index = 0; index < leds.Count; index++)
            {
                ArtemisLed artemisLed = leds[index];
                if ((ColorFirstLedOnly && index == 0) || !ColorFirstLedOnly)
                {
                    paint.Color = listDeviceViewModel.Color.WithAlpha(alpha);
                    e.Canvas.DrawRect(artemisLed.AbsoluteRectangle, paint);
                }
            }
        }
    }

    #region Context menu commands

    private void ExecuteIdentifyDevice(ArtemisDevice device)
    {
        _deviceService.IdentifyDevice(device);
    }

    private async Task ExecuteViewProperties(ArtemisDevice device)
    {
        await _windowService.ShowDialogAsync(_deviceVmFactory.DevicePropertiesViewModel(device));
    }

    private void ExecuteBringToFront(ArtemisDevice device)
    {
        SurfaceDeviceViewModel surfaceDeviceViewModel = SurfaceDeviceViewModels.First(d => d.Device == device);
        SurfaceDeviceViewModels.Move(SurfaceDeviceViewModels.IndexOf(surfaceDeviceViewModel), SurfaceDeviceViewModels.Count - 1);
        for (int i = 0; i < SurfaceDeviceViewModels.Count; i++)
        {
            SurfaceDeviceViewModel deviceViewModel = SurfaceDeviceViewModels[i];
            deviceViewModel.Device.ZIndex = i + 1;
        }

        ListDeviceViewModels.Sort(l => l.Device.ZIndex * -1);

        _deviceService.SaveDevices();
    }

    private void ExecuteBringForward(ArtemisDevice device)
    {
        SurfaceDeviceViewModel surfaceDeviceViewModel = SurfaceDeviceViewModels.First(d => d.Device == device);
        int currentIndex = SurfaceDeviceViewModels.IndexOf(surfaceDeviceViewModel);
        int newIndex = Math.Min(currentIndex + 1, SurfaceDeviceViewModels.Count - 1);
        SurfaceDeviceViewModels.Move(currentIndex, newIndex);

        for (int i = 0; i < SurfaceDeviceViewModels.Count; i++)
        {
            SurfaceDeviceViewModel deviceViewModel = SurfaceDeviceViewModels[i];
            deviceViewModel.Device.ZIndex = i + 1;
        }

        ListDeviceViewModels.Sort(l => l.Device.ZIndex * -1);

        _deviceService.SaveDevices();
    }

    private void ExecuteSendToBack(ArtemisDevice device)
    {
        SurfaceDeviceViewModel surfaceDeviceViewModel = SurfaceDeviceViewModels.First(d => d.Device == device);
        SurfaceDeviceViewModels.Move(SurfaceDeviceViewModels.IndexOf(surfaceDeviceViewModel), 0);
        for (int i = 0; i < SurfaceDeviceViewModels.Count; i++)
        {
            SurfaceDeviceViewModel deviceViewModel = SurfaceDeviceViewModels[i];
            deviceViewModel.Device.ZIndex = i + 1;
        }

        ListDeviceViewModels.Sort(l => l.Device.ZIndex * -1);

        _deviceService.SaveDevices();
    }

    private void ExecuteSendBackward(ArtemisDevice device)
    {
        SurfaceDeviceViewModel surfaceDeviceViewModel = SurfaceDeviceViewModels.First(d => d.Device == device);
        int currentIndex = SurfaceDeviceViewModels.IndexOf(surfaceDeviceViewModel);
        int newIndex = Math.Max(currentIndex - 1, 0);
        SurfaceDeviceViewModels.Move(currentIndex, newIndex);
        for (int i = 0; i < SurfaceDeviceViewModels.Count; i++)
        {
            SurfaceDeviceViewModel deviceViewModel = SurfaceDeviceViewModels[i];
            deviceViewModel.Device.ZIndex = i + 1;
        }

        ListDeviceViewModels.Sort(l => l.Device.ZIndex * -1);

        _deviceService.SaveDevices();
    }

    #endregion
}