using System.IO;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using RGB.NET.Devices.SteelSeries;

namespace Artemis.Plugins.Devices.SteelSeries
{
    // ReSharper disable once UnusedMember.Global
    public class SteelSeriesDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;

        public SteelSeriesDeviceProvider(PluginInfo pluginInfo, IRgbService rgbService) : base(pluginInfo, RGB.NET.Devices.SteelSeries.SteelSeriesDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            // TODO Check to see if this works, it's usually a generic type after all
            PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(SteelSeriesRGBDevice), sender, args);
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }

        public override void DisablePlugin()
        {
            // TODO: Remove the device provider from the surface
        }

        public override void Dispose()
        {
            // TODO: This will probably not go well without first removing the device provider
            // SteelSeriesDeviceProvider.Instance.ResetDevices();
            // SteelSeriesDeviceProvider.Instance.Dispose();
        }
    }
}