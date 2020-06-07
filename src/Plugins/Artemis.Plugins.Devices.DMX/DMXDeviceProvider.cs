using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Services.Interfaces;
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
            HasConfigurationViewModel = true;
        }

        public override void EnablePlugin()
        {
            // TODO: Load from configuration
            // RGB.NET.Devices.DMX.DMXDeviceProvider.Instance.AddDeviceDefinition();
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }

        public override PluginConfigurationViewModel GetConfigurationViewModel()
        {
            return new DMXConfigurationViewModel(this);
        }
    }
}