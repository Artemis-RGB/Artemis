using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using Artemis.Plugins.Devices.DMX.ViewModels;

namespace Artemis.Plugins.Devices.DMX
{
    // ReSharper disable once UnusedMember.Global
    public class DMXDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;

        public DMXDeviceProvider(IRgbService rgbService) : base(RGB.NET.Devices.DMX.DMXDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            ConfigurationDialog = new PluginConfigurationDialog<DMXConfigurationViewModel>();

            // TODO: Load from configuration
            // RGB.NET.Devices.DMX.DMXDeviceProvider.Instance.AddDeviceDefinition();
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }
    }
}