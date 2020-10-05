using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using Artemis.Plugins.Devices.WS281X.Settings;
using Artemis.Plugins.Devices.WS281X.ViewModels;
using RGB.NET.Devices.WS281X.Arduino;
using RGB.NET.Devices.WS281X.Bitwizard;

namespace Artemis.Plugins.Devices.WS281X
{
    // ReSharper disable once UnusedMember.Global
    public class WS281XDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;
        private readonly PluginSettings _settings;

        public WS281XDeviceProvider(IRgbService rgbService, PluginSettings settings) : base(RGB.NET.Devices.WS281X.WS281XDeviceProvider.Instance)
        {
            _settings = settings;
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            ConfigurationDialog = new PluginConfigurationDialog<WS281XConfigurationViewModel>();

            PluginSetting<List<DeviceDefinition>> definitions = _settings.GetSetting<List<DeviceDefinition>>("DeviceDefinitions");
            if (definitions.Value == null)
                definitions.Value = new List<DeviceDefinition>();

            foreach (DeviceDefinition deviceDefinition in definitions.Value)
            {
                switch (deviceDefinition.Type)
                {
                    case DeviceDefinitionType.Arduino:
                        RGB.NET.Devices.WS281X.WS281XDeviceProvider.Instance.AddDeviceDefinition(new ArduinoWS281XDeviceDefinition(deviceDefinition.Port));
                        break;
                    case DeviceDefinitionType.Bitwizard:
                        RGB.NET.Devices.WS281X.WS281XDeviceProvider.Instance.AddDeviceDefinition(new BitwizardWS281XDeviceDefinition(deviceDefinition.Port));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }

        public override void DisablePlugin()
        {
            // TODO: Remove the device provider from the surface
        }
    }
}