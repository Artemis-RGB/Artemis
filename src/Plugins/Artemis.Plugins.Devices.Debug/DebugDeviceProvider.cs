using System;
using System.Collections.Generic;
using System.IO;
using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using Artemis.Plugins.Devices.Debug.Settings;
using Artemis.Plugins.Devices.Debug.ViewModels;
using RGB.NET.Core;
using RGB.NET.Devices.Debug;
using Serilog;

namespace Artemis.Plugins.Devices.Debug
{
    // ReSharper disable once UnusedMember.Global
    public class DebugDeviceProvider : DeviceProvider
    {
        private readonly ILogger _logger;
        private readonly IRgbService _rgbService;
        private readonly PluginSettings _settings;

        public DebugDeviceProvider(IRgbService rgbService, PluginSettings settings, ILogger logger) : base(RGB.NET.Devices.Debug.DebugDeviceProvider.Instance)
        {
            _settings = settings;
            _logger = logger;
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            ConfigurationDialog = new PluginConfigurationDialog<DebugConfigurationViewModel>();
            PathHelper.ResolvingAbsolutePath += PathHelperOnResolvingAbsolutePath;

            var definitions = _settings.GetSetting("DeviceDefinitions", new List<DeviceDefinition>());
            if (definitions.Value == null)
                definitions.Value = new List<DeviceDefinition>();

            foreach (var deviceDefinition in definitions.Value)
                RGB.NET.Devices.Debug.DebugDeviceProvider.Instance.AddFakeDeviceDefinition(deviceDefinition.Layout, deviceDefinition.ImageLayout);

            try
            {
                _rgbService.AddDeviceProvider(RgbDeviceProvider);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Debug device provided failed to initialize, check paths");
            }
        }

        public override void DisablePlugin()
        {
            // TODO: Remove the device provider from the surface
        }

        private void PathHelperOnResolvingAbsolutePath(object sender, ResolvePathEventArgs e)
        {
            if (sender is DebugRGBDevice debugRgbDevice)
            {
                if (debugRgbDevice.LayoutPath.Contains("\\Layouts\\"))
                {
                    var rootDirectory = debugRgbDevice.LayoutPath.Split("\\Layouts")[0];
                    e.FinalPath = Path.Combine(rootDirectory, e.RelativePath);
                }
            }
        }
    }
}