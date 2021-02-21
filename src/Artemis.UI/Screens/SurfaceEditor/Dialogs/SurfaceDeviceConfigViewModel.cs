using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Screens.SurfaceEditor.Dialogs
{
    public class SurfaceDeviceConfigViewModel : DialogViewModelBase
    {
        private readonly ICoreService _coreService;
        private readonly IRgbService _rgbService;
        private readonly IMessageService _messageService;
        private readonly double _initialRedScale;
        private readonly double _initialGreenScale;
        private readonly double _initialBlueScale;
        private int _rotation;
        private double _scale;
        private int _x;
        private int _y;
        private double _redScale;
        private double _greenScale;
        private double _blueScale;
        private SKColor _currentColor;
        private bool _displayOnDevices;

        public SurfaceDeviceConfigViewModel(ArtemisDevice device,
            ICoreService coreService,
            IRgbService rgbService,
            IMessageService messageService,
            IModelValidator<SurfaceDeviceConfigViewModel> validator) : base(validator)
        {
            _coreService = coreService;
            _rgbService = rgbService;
            _messageService = messageService;

            Device = device;

            X = (int) Device.X;
            Y = (int) Device.Y;
            Scale = Device.Scale;
            Rotation = (int) Device.Rotation;
            RedScale = Device.RedScale * 100d;
            GreenScale = Device.GreenScale * 100d;
            BlueScale = Device.BlueScale * 100d;
            //we need to store the initial values to be able to restore them when the user clicks "Cancel"
            _initialRedScale = Device.RedScale;
            _initialGreenScale = Device.GreenScale;
            _initialBlueScale = Device.BlueScale;
            CurrentColor = SKColors.White;
            _coreService.FrameRendering += OnFrameRendering;
            Device.PropertyChanged += DeviceOnPropertyChanged;
        }

        public ArtemisDevice Device { get; }

        public override void OnDialogClosed(object sender, DialogClosingEventArgs e)
        {
            _coreService.FrameRendering -= OnFrameRendering;
            Device.PropertyChanged -= DeviceOnPropertyChanged;
            base.OnDialogClosed(sender, e);
        }

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

        public double Scale
        {
            get => _scale;
            set => SetAndNotify(ref _scale, value);
        }

        public int Rotation
        {
            get => _rotation;
            set => SetAndNotify(ref _rotation, value);
        }

        public double RedScale
        {
            get => _redScale;
            set => SetAndNotify(ref _redScale, value);
        }

        public double GreenScale
        {
            get => _greenScale;
            set => SetAndNotify(ref _greenScale, value);
        }

        public double BlueScale
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

        public async Task Accept()
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
            Device.RedScale = RedScale / 100d;
            Device.GreenScale = GreenScale / 100d;
            Device.BlueScale = BlueScale / 100d;

            _coreService.ModuleRenderingDisabled = false;
            Session.Close(true);
        }

        public void ApplyScaling()
        {
            Device.RedScale = RedScale / 100d;
            Device.GreenScale = GreenScale / 100d;
            Device.BlueScale = BlueScale / 100d;
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

        public override void Cancel()
        {
            Device.RedScale = _initialRedScale;
            Device.GreenScale = _initialGreenScale;
            Device.BlueScale = _initialBlueScale;

            base.Cancel();
        }

        private void DeviceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Device.CustomLayoutPath))
            {
                _rgbService.ApplyBestDeviceLayout(Device);
            }
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
    }
}