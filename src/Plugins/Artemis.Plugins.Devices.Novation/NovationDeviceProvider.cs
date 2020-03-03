using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using RGB.NET.Devices.Novation;

namespace Artemis.Plugins.Devices.Novation
{
    // ReSharper disable once UnusedMember.Global
    public class NovationDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;

        public NovationDeviceProvider(PluginInfo pluginInfo, IRgbService rgbService) : base(pluginInfo, RGB.NET.Devices.Novation.NovationDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(NovationRGBDevice<>), sender, args);
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }
    }
}