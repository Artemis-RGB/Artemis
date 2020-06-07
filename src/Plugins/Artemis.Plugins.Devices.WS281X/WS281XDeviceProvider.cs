using System;
using System.Collections.Generic;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
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

        public WS281XDeviceProvider(PluginInfo pluginInfo, IRgbService rgbService, PluginSettings settings) : base(pluginInfo, RGB.NET.Devices.WS281X.WS281XDeviceProvider.Instance)
        {
            Settings = settings;
            _rgbService = rgbService;
            HasConfigurationViewModel = true;
        }

        public PluginSettings Settings { get; }

        public override void EnablePlugin()
        {
            var definitions = Settings.GetSetting<List<DeviceDefinition>>("DeviceDefinitions");
            if (definitions.Value == null)
                definitions.Value = new List<DeviceDefinition>();

            foreach (var deviceDefinition in definitions.Value)
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
        
        public override PluginConfigurationViewModel GetConfigurationViewModel()
        {
            return new WS281XConfigurationViewModel(this, Settings);
        }
    }
}