using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Devices.WS281X.ViewModels;

namespace Artemis.Plugins.Devices.WS281X
{
    // ReSharper disable once UnusedMember.Global
    public class WS281XDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;

        public WS281XDeviceProvider(PluginInfo pluginInfo, IRgbService rgbService) : base(pluginInfo, RGB.NET.Devices.WS281X.WS281XDeviceProvider.Instance)
        {
            _rgbService = rgbService;
            HasConfigurationViewModel = true;
        }

        public override void EnablePlugin()
        {
            // TODO: Load from configuration
            //RGB.NET.Devices.WS281X.WS281XDeviceProvider.Instance.AddDeviceDefinition();
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }

        public override void DisablePlugin()
        {
            // TODO: Remove the device provider from the surface
        }

        public override void Dispose()
        {
            // TODO: This will probably not go well without first removing the device provider
            // WS281XDeviceProvider.Instance.ResetDevices();
            // WS281XDeviceProvider.Instance.Dispose();
        }

        public override PluginConfigurationViewModel GetConfigurationViewModel()
        {
            return new WS281XConfigurationViewModel(this);
        }
    }
}