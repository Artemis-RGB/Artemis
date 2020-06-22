using System.Threading.Tasks;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Screens.SurfaceEditor.Visualization;
using Artemis.UI.Shared.Services.Dialog;
using Stylet;

namespace Artemis.UI.Screens.SurfaceEditor.Dialogs
{
    public class SurfaceDeviceConfigViewModel : DialogViewModelBase
    {
        private readonly ICoreService _coreService;

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
        public string Title { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public double Scale { get; set; }
        public int Rotation { get; set; }

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