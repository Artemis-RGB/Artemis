using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
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

        public SurfaceDeviceConfigViewModel(ArtemisDevice device, ICoreService coreService, IModelValidator<SurfaceDeviceConfigViewModel> validator) : base(validator)
        {
            _coreService = coreService;

            Device = device;
            Title = $"{Device.RgbDevice.DeviceInfo.DeviceName} - Properties";

            X = (int) Device.X;
            Y = (int) Device.Y;
            Scale = Device.Scale;
            Rotation = (int) Device.Rotation;
        }

        public ArtemisDevice Device { get; }


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

            Device.X = X;
            Device.Y = Y;
            Device.Scale = Scale;
            Device.Rotation = Rotation;

            _coreService.ModuleRenderingDisabled = false;

            Session.Close(true);
        }
    }
}