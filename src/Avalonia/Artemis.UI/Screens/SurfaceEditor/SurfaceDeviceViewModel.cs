using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using Avalonia.Input;
using ReactiveUI;
using RGB.NET.Core;
using SkiaSharp;
using Point = Avalonia.Point;

namespace Artemis.UI.Screens.SurfaceEditor
{
    public class SurfaceDeviceViewModel : ActivatableViewModelBase
    {
        private readonly IRgbService _rgbService;
        private readonly IDeviceService _deviceService;
        private readonly ISettingsService _settingsService;
        private readonly IDeviceVmFactory _deviceVmFactory;
        private readonly IWindowService _windowService;
        private Cursor _cursor;
        private double _dragOffsetX;
        private double _dragOffsetY;
        private SelectionStatus _selectionStatus;

        public SurfaceDeviceViewModel(ArtemisDevice device, IRgbService rgbService, IDeviceService deviceService, ISettingsService settingsService, IDeviceVmFactory deviceVmFactory, IWindowService windowService)
        {
            _rgbService = rgbService;
            _deviceService = deviceService;
            _settingsService = settingsService;
            _deviceVmFactory = deviceVmFactory;
            _windowService = windowService;
            _cursor = Cursor.Default;

            Device = device;

            IdentifyDevice = ReactiveCommand.Create<ArtemisDevice>(ExecuteIdentifyDevice);
            ViewProperties = ReactiveCommand.CreateFromTask<ArtemisDevice>(ExecuteViewProperties);
        }

        public ReactiveCommand<ArtemisDevice, Unit> IdentifyDevice { get; }
        public ReactiveCommand<ArtemisDevice, Unit> ViewProperties { get; }

        public ArtemisDevice Device { get; }

        public SelectionStatus SelectionStatus
        {
            get => _selectionStatus;
            set
            {
                RaiseAndSetIfChanged(ref _selectionStatus, value);
                this.RaisePropertyChanged(nameof(Highlighted));
            }
        }

        public bool Highlighted => SelectionStatus != SelectionStatus.None;

        public bool CanDetectInput => Device.DeviceType == RGBDeviceType.Keyboard || Device.DeviceType == RGBDeviceType.Mouse;

        public Cursor Cursor
        {
            get => _cursor;
            set => RaiseAndSetIfChanged(ref _cursor, value);
        }

        public void StartMouseDrag(Point mouseStartPosition)
        {
            if (SelectionStatus != SelectionStatus.Selected)
                return;

            _dragOffsetX = Device.X - mouseStartPosition.X;
            _dragOffsetY = Device.Y - mouseStartPosition.Y;
        }

        public void UpdateMouseDrag(Point mousePosition)
        {
            if (SelectionStatus != SelectionStatus.Selected)
                return;

            float roundedX = (float) Math.Round((mousePosition.X + _dragOffsetX) / 10d, 0, MidpointRounding.AwayFromZero) * 10f;
            float roundedY = (float) Math.Round((mousePosition.Y + _dragOffsetY) / 10d, 0, MidpointRounding.AwayFromZero) * 10f;

            if (Fits(roundedX, roundedY))
            {
                Device.X = roundedX;
                Device.Y = roundedY;
            }
            else if (Fits(roundedX, Device.Y))
            {
                Device.X = roundedX;
            }
            else if (Fits(Device.X, roundedY))
            {
                Device.Y = roundedY;
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

        private bool Fits(float x, float y)
        {
            if (x < 0 || y < 0)
                return false;

            double maxTextureSize = 4096 / _settingsService.GetSetting("Core.RenderScale", 0.25).Value;
            if (x + Device.Rectangle.Width > maxTextureSize || y + Device.Rectangle.Height > maxTextureSize)
                return false;

            List<SKRect> own = Device.Leds
                .Select(l => SKRect.Create(l.Rectangle.Left + x, l.Rectangle.Top + y, l.Rectangle.Width, l.Rectangle.Height))
                .ToList();
            List<SKRect> others = _rgbService.EnabledDevices
                .Where(d => d != Device && d.IsEnabled)
                .SelectMany(d => d.Leds)
                .Select(l => SKRect.Create(l.Rectangle.Left + l.Device.X, l.Rectangle.Top + l.Device.Y, l.Rectangle.Width, l.Rectangle.Height))
                .ToList();


            return !own.Any(o => others.Any(l => l.IntersectsWith(o)));
        }
    }

    public enum SelectionStatus
    {
        None,
        Hover,
        Selected
    }
}