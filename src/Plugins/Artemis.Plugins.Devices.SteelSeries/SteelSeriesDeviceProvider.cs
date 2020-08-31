using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using RGB.NET.Core;
using RGB.NET.Devices.SteelSeries;

namespace Artemis.Plugins.Devices.SteelSeries
{
    // ReSharper disable once UnusedMember.Global
    public class SteelSeriesDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;

        public SteelSeriesDeviceProvider(IRgbService rgbService) : base(RGB.NET.Devices.SteelSeries.SteelSeriesDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            // TODO Check to see if this works, it's usually a generic type after all
            PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(SteelSeriesRGBDevice), sender, args);
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }
    }
}