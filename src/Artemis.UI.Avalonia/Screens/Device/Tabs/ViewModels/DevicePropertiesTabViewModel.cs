using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Avalonia.Shared;
using Artemis.UI.Avalonia.Shared.Services.Builders;
using Artemis.UI.Avalonia.Shared.Services.Interfaces;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Avalonia.Screens.Device.Tabs.ViewModels
{
    public class DevicePropertiesTabViewModel : ActivatableViewModelBase
    {
        private readonly List<DeviceCategory> _categories;
        private readonly ICoreService _coreService;
        private readonly float _initialBlueScale;
        private readonly float _initialGreenScale;
        private readonly float _initialRedScale;
        private readonly INotificationService _notificationService;
        private readonly IRgbService _rgbService;
        private readonly IWindowService _windowService;
        private float _blueScale;
        private SKColor _currentColor;
        private bool _displayOnDevices;
        private float _greenScale;
        private float _redScale;
        private int _rotation;
        private float _scale;
        private int _x;
        private int _y;

#pragma warning disable CS8618 // Design-time constructor
        public DevicePropertiesTabViewModel()
        {
        }
#pragma warning restore CS8618

        public DevicePropertiesTabViewModel(ArtemisDevice device, ICoreService coreService, IRgbService rgbService, IWindowService windowService, INotificationService notificationService)
        {
            _coreService = coreService;
            _rgbService = rgbService;
            _windowService = windowService;
            _notificationService = notificationService;
            _categories = new List<DeviceCategory>(device.Categories);

            Device = device;
            DisplayName = "Properties";

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

            Device.PropertyChanged += DeviceOnPropertyChanged;
            _coreService.FrameRendering += OnFrameRendering;
        }

        public ArtemisDevice Device { get; }

        public int X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        public int Y
        {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        public float Scale
        {
            get => _scale;
            set => this.RaiseAndSetIfChanged(ref _scale, value);
        }

        public int Rotation
        {
            get => _rotation;
            set => this.RaiseAndSetIfChanged(ref _rotation, value);
        }

        public float RedScale
        {
            get => _redScale;
            set => this.RaiseAndSetIfChanged(ref _redScale, value);
        }

        public float GreenScale
        {
            get => _greenScale;
            set => this.RaiseAndSetIfChanged(ref _greenScale, value);
        }

        public float BlueScale
        {
            get => _blueScale;
            set => this.RaiseAndSetIfChanged(ref _blueScale, value);
        }

        public SKColor CurrentColor
        {
            get => _currentColor;
            set => this.RaiseAndSetIfChanged(ref _currentColor, value);
        }

        public bool DisplayOnDevices
        {
            get => _displayOnDevices;
            set => this.RaiseAndSetIfChanged(ref _displayOnDevices, value);
        }

        // This solution won't scale well but I don't expect there to be many more categories.
        // If for some reason there will be, dynamically creating a view model per category may be more appropriate
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

        public void ApplyScaling()
        {
            Device.RedScale = RedScale / 100f;
            Device.GreenScale = GreenScale / 100f;
            Device.BlueScale = BlueScale / 100f;

            _rgbService.FlushLeds = true;
        }

        public void ClearCustomLayout()
        {
            Device.CustomLayoutPath = null;
            _notificationService.CreateNotification()
                .WithMessage("Cleared imported layout.")
                .WithSeverity(NotificationSeverity.Informational);
        }

        public async Task BrowseCustomLayout()
        {
            string[]? files = await _windowService.CreateOpenFileDialog()
                .WithTitle("Select device layout file")
                .HavingFilter(f => f.WithName("Layout files").WithExtension("xml"))
                .ShowAsync();

            if (files?.Length > 0)
            {
                Device.CustomLayoutPath = files[0];
                _notificationService.CreateNotification()
                    .WithTitle("Imported layout")
                    .WithMessage($"File loaded from {files[0]}")
                    .WithSeverity(NotificationSeverity.Informational);
            }
        }

        public async Task SelectPhysicalLayout()
        {
            // await _windowService.CreateContentDialog()
            //     .WithTitle("Select layout")
            //     .WithViewModel<DeviceLayoutDialogViewModel>(("device", Device))
            //     .ShowAsync();
        }

        public async Task Apply()
        {
            // TODO: Validation

            _coreService.ProfileRenderingDisabled = true;
            await Task.Delay(100);

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

            _rgbService.SaveDevice(Device);

            _coreService.ProfileRenderingDisabled = false;
        }

        public void Reset()
        {
            HasDeskCategory = Device.Categories.Contains(DeviceCategory.Desk);
            HasMonitorCategory = Device.Categories.Contains(DeviceCategory.Monitor);
            HasCaseCategory = Device.Categories.Contains(DeviceCategory.Case);
            HasRoomCategory = Device.Categories.Contains(DeviceCategory.Room);
            HasPeripheralsCategory = Device.Categories.Contains(DeviceCategory.Peripherals);

            Device.RedScale = _initialRedScale;
            Device.GreenScale = _initialGreenScale;
            Device.BlueScale = _initialBlueScale;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _coreService.FrameRendering -= OnFrameRendering;
                Device.PropertyChanged -= DeviceOnPropertyChanged;
            }

            base.Dispose(disposing);
        }

        private bool GetCategory(DeviceCategory category)
        {
            return _categories.Contains(category);
        }

        private void SetCategory(DeviceCategory category, bool value)
        {
            if (value && !_categories.Contains(category))
                _categories.Add(category);
            else
                _categories.Remove(category);

            this.RaisePropertyChanged($"Has{category}Category");
        }

        private void OnFrameRendering(object? sender, FrameRenderingEventArgs e)
        {
            if (!_displayOnDevices)
                return;

            using SKPaint overlayPaint = new() {Color = CurrentColor};
            e.Canvas.DrawRect(0, 0, e.Canvas.LocalClipBounds.Width, e.Canvas.LocalClipBounds.Height, overlayPaint);
        }

        private void DeviceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Device.CustomLayoutPath) || e.PropertyName == nameof(Device.DisableDefaultLayout))
                Task.Run(() => _rgbService.ApplyBestDeviceLayout(Device));
        }
    }
}