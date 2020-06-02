using System.IO;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using RGB.NET.Devices.Wooting.Generic;

namespace Artemis.Plugins.Devices.Wooting
{
    // ReSharper disable once UnusedMember.Global
    public class WootingDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;

        public WootingDeviceProvider(PluginInfo pluginInfo, IRgbService rgbService) : base(pluginInfo, RGB.NET.Devices.Wooting.WootingDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        protected override void EnablePlugin()
        {
            PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(WootingRGBDevice<>), sender, args);
            RGB.NET.Devices.Wooting.WootingDeviceProvider.PossibleX64NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x64", "wooting-rgb-sdk64.dll"));
            RGB.NET.Devices.Wooting.WootingDeviceProvider.PossibleX86NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x86", "wooting-rgb-sdk.dll"));
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }

        protected override void DisablePlugin()
        {
            // TODO: Remove the device provider from the surface
        }
    }
}