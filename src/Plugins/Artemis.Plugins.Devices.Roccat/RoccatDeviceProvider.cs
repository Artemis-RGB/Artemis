using System.IO;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using RGB.NET.Devices.Roccat;

namespace Artemis.Plugins.Devices.Roccat
{
    // ReSharper disable once UnusedMember.Global
    public class RoccatDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;

        public RoccatDeviceProvider(PluginInfo pluginInfo, IRgbService rgbService) : base(pluginInfo, RGB.NET.Devices.Roccat.RoccatDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            // TODO: Find out why this is missing, Roccat seems unimplemented
            // PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(RoccatRGBDevice<>), sender, args);
            RGB.NET.Devices.Roccat.RoccatDeviceProvider.PossibleX64NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x64", "RoccatTalkSDKWrapper.dll"));
            RGB.NET.Devices.Roccat.RoccatDeviceProvider.PossibleX86NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x86", "RoccatTalkSDKWrapper.dll"));
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }

        public override void DisablePlugin()
        {
            // TODO: Remove the device provider from the surface
        }

        public override void Dispose()
        {
            // TODO: This will probably not go well without first removing the device provider
            // RoccatDeviceProvider.Instance.ResetDevices();
            // RoccatDeviceProvider.Instance.Dispose();
        }
    }
}