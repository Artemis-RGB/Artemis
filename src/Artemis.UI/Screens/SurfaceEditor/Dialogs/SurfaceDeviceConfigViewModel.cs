using System.Threading.Tasks;
using Artemis.Core.Services;
using Artemis.UI.Screens.SurfaceEditor.Visualization;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.SurfaceEditor.Dialogs
{
    public class SurfaceDeviceConfigViewModel : DialogViewModelBase
    {
        private readonly ICoreService _coreService;
        private int _rotation;
        private double _scale;
        private string _title;
        private int _x;
        private int _y;

        public SurfaceDeviceConfigViewModel(SurfaceDeviceViewModel surfaceDeviceViewModel, ICoreService coreService, IModelValidator<SurfaceDeviceConfigViewModel> validator)
            : base(validator)
        {
            _coreService = coreService;
            SurfaceDeviceViewModel = surfaceDeviceViewModel;
            Title = $"{SurfaceDeviceViewModel.Device.RgbDevice.DeviceInfo.DeviceName} - Properties";

            X = (int) SurfaceDeviceViewModel.Device.X;
            Y = (int) SurfaceDeviceViewModel.Device.Y;
            Scale = SurfaceDeviceViewModel.Device.Scale;
            Rotation = (int) SurfaceDeviceViewModel.Device.Rotation;
        }

        public SurfaceDeviceViewModel SurfaceDeviceViewModel { get; }

        public string Title
        {
            get => _title;
            set => SetAndNotify(ref _title, value);
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

        public async Task Accept()
        {
            await ValidateAsync();
            if (HasErrors)
                return;

            _coreService.ModuleRenderingDisabled = true;
            await Task.Delay(100);

            SurfaceDeviceViewModel.Device.X = X;
            SurfaceDeviceViewModel.Device.Y = Y;
            SurfaceDeviceViewModel.Device.Scale = Scale;
            SurfaceDeviceViewModel.Device.Rotation = Rotation;

            _coreService.ModuleRenderingDisabled = false;

            Session.Close(true);
        }

        public void Cancel()
        {
            Session.Close(false);
        }
    }
}