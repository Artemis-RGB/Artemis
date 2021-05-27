using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using Ookii.Dialogs.Wpf;
using RGB.NET.Core;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Screens.Settings.Device.Tabs
{
    public class DevicePropertiesTabViewModel : Screen
    {
        private readonly ICoreService _coreService;
        private readonly IDialogService _dialogService;
        private readonly IRgbService _rgbService;
        private float _blueScale;
        private SKColor _currentColor;
        private bool _displayOnDevices;
        private float _greenScale;
        private List<DeviceCategory> _categories;
        private float _initialBlueScale;
        private float _initialGreenScale;
        private float _initialRedScale;
        private float _redScale;
        private int _rotation;
        private float _scale;
        private int _x;
        private int _y;

        public DevicePropertiesTabViewModel(ArtemisDevice device,
            ICoreService coreService,
            IRgbService rgbService,
            IDialogService dialogService,
            IModelValidator<DevicePropertiesTabViewModel> validator) : base(validator)
        {
            _coreService = coreService;
            _rgbService = rgbService;
            _dialogService = dialogService;

            Device = device;
            DisplayName = "PROPERTIES";
        }

        public ArtemisDevice Device { get; }

        public int X
        {
            get => _x;
            set => SetAndNotify(ref _x, value);
        }

        public int Y
        {
            get => _y;
            set => SetAndNotify(ref _y, value);
        }

        public float Scale
        {
            get => _scale;
            set => SetAndNotify(ref _scale, value);
        }

        public int Rotation
        {
            get => _rotation;
            set => SetAndNotify(ref _rotation, value);
        }

        public float RedScale
        {
            get => _redScale;
            set => SetAndNotify(ref _redScale, value);
        }

        public float GreenScale
        {
            get => _greenScale;
            set => SetAndNotify(ref _greenScale, value);
        }

        public float BlueScale
        {
            get => _blueScale;
            set => SetAndNotify(ref _blueScale, value);
        }

        public SKColor CurrentColor
        {
            get => _currentColor;
            set => SetAndNotify(ref _currentColor, value);
        }

        public bool DisplayOnDevices
        {
            get => _displayOnDevices;
            set => SetAndNotify(ref _displayOnDevices, value);
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

            _rgbService.Surface.Update(true);
        }

        public void BrowseCustomLayout(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is Button)
            {
                Device.CustomLayoutPath = null;
                ((DeviceDialogViewModel) Parent).DeviceMessageQueue.Enqueue("Cleared imported layout.");
                return;
            }

            VistaOpenFileDialog dialog = new();
            dialog.Filter = "Layout files (*.xml)|*.xml";
            dialog.Title = "Select device layout file";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                Device.CustomLayoutPath = dialog.FileName;
                ((DeviceDialogViewModel) Parent).DeviceMessageQueue.Enqueue($"Imported layout from {dialog.FileName}.");
            }
        }

        public async Task SelectPhysicalLayout()
        {
            await _dialogService.ShowDialogAt<DeviceLayoutDialogViewModel>("DeviceDialog", new Dictionary<string, object> {{"device", Device}});
        }

        public async Task Apply()
        {
            await ValidateAsync();
            if (HasErrors)
                return;

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

        protected override void OnActivate()
        {
            X = (int) Device.X;
            Y = (int) Device.Y;
            Scale = Device.Scale;
            Rotation = (int) Device.Rotation;
            RedScale = Device.RedScale * 100f;
            GreenScale = Device.GreenScale * 100f;
            BlueScale = Device.BlueScale * 100f;
            //we need to store the initial values to be able to restore them when the user clicks "Cancel"
            _initialRedScale = Device.RedScale;
            _initialGreenScale = Device.GreenScale;
            _initialBlueScale = Device.BlueScale;
            _categories = new List<DeviceCategory>(Device.Categories);
            CurrentColor = SKColors.White;

            _coreService.FrameRendering += OnFrameRendering;
            Device.PropertyChanged += DeviceOnPropertyChanged;

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            _coreService.FrameRendering -= OnFrameRendering;
            Device.PropertyChanged -= DeviceOnPropertyChanged;

            base.OnDeactivate();
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

            NotifyOfPropertyChange($"Has{category}Category");
        }

        #region Event handlers

        private void DeviceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Device.CustomLayoutPath))
                _rgbService.ApplyBestDeviceLayout(Device);
        }

        private void OnFrameRendering(object sender, FrameRenderingEventArgs e)
        {
            if (!_displayOnDevices)
                return;

            using SKPaint overlayPaint = new()
            {
                Color = CurrentColor
            };
            e.Canvas.DrawRect(0, 0, e.Canvas.LocalClipBounds.Width, e.Canvas.LocalClipBounds.Height, overlayPaint);
        }

        #endregion
    }
}