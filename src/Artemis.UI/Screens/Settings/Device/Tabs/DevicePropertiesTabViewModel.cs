using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using Ookii.Dialogs.Wpf;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Screens.Settings.Device.Tabs
{
    public class DevicePropertiesTabViewModel : Screen
    {
        private readonly ICoreService _coreService;
        private readonly IMessageService _messageService;
        private readonly IDialogService _dialogService;
        private readonly IRgbService _rgbService;
        private float _blueScale;
        private SKColor _currentColor;
        private bool _displayOnDevices;
        private float _greenScale;
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
            IMessageService messageService,
            IDialogService dialogService,
            IModelValidator<DevicePropertiesTabViewModel> validator) : base(validator)
        {
            _coreService = coreService;
            _rgbService = rgbService;
            _messageService = messageService;
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

        public void ApplyScaling()
        {
            Device.RedScale = RedScale / 100f;
            Device.GreenScale = GreenScale / 100f;
            Device.BlueScale = BlueScale / 100f;
        }

        public void BrowseCustomLayout(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is Button)
            {
                Device.CustomLayoutPath = null;
                _messageService.ShowMessage("Cleared imported layout");
                return;
            }

            VistaOpenFileDialog dialog = new();
            dialog.Filter = "Layout files (*.xml)|*.xml";
            dialog.Title = "Select device layout file";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                Device.CustomLayoutPath = dialog.FileName;
                _messageService.ShowMessage($"Imported layout from {dialog.FileName}");
            }
        }

        public async Task SelectPhysicalLayout()
        {
            await _dialogService.ShowDialog<DeviceLayoutDialogViewModel>(new Dictionary<string, object> {{"device", Device}});
        }

        public async Task Apply()
        {
            await ValidateAsync();
            if (HasErrors)
                return;

            _coreService.ModuleRenderingDisabled = true;
            await Task.Delay(100);

            Device.X = X;
            Device.Y = Y;
            Device.Scale = Scale;
            Device.Rotation = Rotation;
            Device.RedScale = RedScale / 100f;
            Device.GreenScale = GreenScale / 100f;
            Device.BlueScale = BlueScale / 100f;

            _coreService.ModuleRenderingDisabled = false;
        }

        public void Reset()
        {
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

        #region Event handlers

        private void DeviceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Device.CustomLayoutPath)) _rgbService.ApplyBestDeviceLayout(Device);
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