using Artemis.Core.Plugins.DeviceProviders;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using RGB.NET.Devices.Asus;
using Serilog;

namespace Artemis.Plugins.Devices.Asus
{
    // ReSharper disable once UnusedMember.Global
    public class AsusDeviceProvider : DeviceProvider
    {
        private readonly ILogger _logger;
        private readonly IRgbService _rgbService;

        public AsusDeviceProvider(IRgbService rgbService, ILogger logger) : base(RGB.NET.Devices.Asus.AsusDeviceProvider.Instance)
        {
            _rgbService = rgbService;
            _logger = logger;
        }

        public override void EnablePlugin()
        {
            _logger.Debug("Expanding ResolvingAbsolutePath");
            PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(AsusRGBDevice<>), sender, args);
            _logger.Debug("Initializing device provider");
            RGB.NET.Devices.Asus.AsusDeviceProvider.Instance.Initialize(RGBDeviceType.All, false, true);
            _logger.Debug("Adding device provider to surface");
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
            _logger.Debug("Added device provider to surface");
        }
    }
}