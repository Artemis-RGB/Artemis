using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Screens.SurfaceEditor.Dialogs
{
    public class SurfaceDeviceConfigViewModel : DialogViewModelBase
    {
        private readonly ICoreService _coreService;
        private int _rotation;
        private double _scale;
        private int _x;
        private int _y;
        public double _redScale;
        public double _greenScale;
        public double _blueScale;
        private SKColor _currentColor;
        private bool _displayOnDevices;

        public SurfaceDeviceConfigViewModel(ArtemisDevice device, ICoreService coreService, IModelValidator<SurfaceDeviceConfigViewModel> validator) : base(validator)
        {
            _coreService = coreService;

            Device = device;

            X = (int)Device.X;
            Y = (int)Device.Y;
            Scale = Device.Scale;
            Rotation = (int)Device.Rotation;
            RedScale = Device.RedScale * 100d;
            GreenScale = Device.GreenScale * 100d;
            BlueScale = Device.BlueScale * 100d;
            CurrentColor = SKColors.White;
            _coreService.FrameRendering += OnFrameRendering;
        }

        private void OnFrameRendering(object sender, FrameRenderingEventArgs e)
        {
            if (!_displayOnDevices)
                return;

            using SKPaint overlayPaint = new SKPaint
            {
                Color = CurrentColor
            };
            e.Canvas.DrawRect(0, 0, e.Canvas.LocalClipBounds.Width, e.Canvas.LocalClipBounds.Height, overlayPaint);
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
            _coreService.FrameRendering -= OnFrameRendering;
            Session.Close(true);
        }

        public void ApplyScaling()
        {
            //TODO: we should either save or revert changes when the user clicks save or cancel.
            //we might have to revert these changes below.
            Device.RedScale = RedScale / 100d;
            Device.GreenScale = GreenScale / 100d;
            Device.BlueScale = BlueScale / 100d;
        }
    }
}