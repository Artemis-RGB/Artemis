using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.ViewModels.Controls.RgbDevice;
using RGB.NET.Core;

namespace Artemis.UI.ViewModels.Controls.Editor
{
    public class SurfaceEditorViewModel
    {
        private readonly IRgbService _rgbService;

        public SurfaceEditorViewModel(IRgbService rgbService)
        {
            Devices = new ObservableCollection<RgbDeviceViewModel>();

            _rgbService = rgbService;
            _rgbService.DeviceLoaded += RgbServiceOnDeviceLoaded;
            _rgbService.Surface.Updated += SurfaceOnUpdated;

            foreach (var surfaceDevice in _rgbService.Surface.Devices)
                Devices.Add(new RgbDeviceViewModel(surfaceDevice));
        }


        public ObservableCollection<RgbDeviceViewModel> Devices { get; set; }

        private void RgbServiceOnDeviceLoaded(object sender, DeviceEventArgs e)
        {
            if (Devices.All(d => d.Device != e.Device))
                Devices.Add(new RgbDeviceViewModel(e.Device));
        }

        private void SurfaceOnUpdated(UpdatedEventArgs args)
        {
            foreach (var rgbDeviceViewModel in Devices)
            {
                rgbDeviceViewModel.Update();
            }
        }
    }
}