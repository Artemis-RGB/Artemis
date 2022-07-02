using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.Device
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

            this.WhenActivated(d =>
            {
                Device.PropertyChanged += DeviceOnPropertyChanged;
                _coreService.FrameRendering += OnFrameRendering;

                Disposable.Create(() =>
                {
                    _coreService.FrameRendering -= OnFrameRendering;
                    Device.PropertyChanged -= DeviceOnPropertyChanged;
                }).DisposeWith(d);
            });
        }

        public ArtemisDevice Device { get; }

        public int X
        {
            get => _x;
            set => RaiseAndSetIfChanged(ref _x, value);
        }

        public int Y
        {
            get => _y;
            set => RaiseAndSetIfChanged(ref _y, value);
        }

        public float Scale
        {
            get => _scale;
            set => RaiseAndSetIfChanged(ref _scale, value);
        }

        public int Rotation
        {
            get => _rotation;
            set => RaiseAndSetIfChanged(ref _rotation, value);
        }

        public float RedScale
        {
            get => _redScale;
            set => RaiseAndSetIfChanged(ref _redScale, value);
        }

        public float GreenScale
        {
            get => _greenScale;
            set => RaiseAndSetIfChanged(ref _greenScale, value);
        }

        public float BlueScale
        {
            get => _blueScale;
            set => RaiseAndSetIfChanged(ref _blueScale, value);
        }

        public SKColor CurrentColor
        {
            get => _currentColor;
            set => RaiseAndSetIfChanged(ref _currentColor, value);
        }

        public bool DisplayOnDevices
        {
            get => _displayOnDevices;
            set => RaiseAndSetIfChanged(ref _displayOnDevices, value);
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

        public bool RequiresManualSetup => !Device.DeviceProvider.CanDetectPhysicalLayout || !Device.DeviceProvider.CanDetectLogicalLayout;

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

        public async Task RestartSetup()
        {
            if (!RequiresManualSetup)
                return;
            if (!Device.DeviceProvider.CanDetectPhysicalLayout && !await DevicePhysicalLayoutDialogViewModel.SelectPhysicalLayout(_windowService, Device))
                return;
            if (!Device.DeviceProvider.CanDetectLogicalLayout && !await DeviceLogicalLayoutDialogViewModel.SelectLogicalLayout(_windowService, Device))
                return;

            await Task.Delay(400);
            _rgbService.SaveDevice(Device);
            _rgbService.ApplyBestDeviceLayout(Device);
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

            RedScale = _initialRedScale * 100;
            GreenScale = _initialGreenScale * 100;
            BlueScale = _initialBlueScale * 100;
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