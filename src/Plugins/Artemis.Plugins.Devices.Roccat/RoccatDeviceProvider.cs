using System.IO;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;

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

        protected override void EnablePlugin()
        {
            // TODO: Find out why this is missing, Roccat seems unimplemented
            // PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(RoccatRGBDevice<>), sender, args);
            RGB.NET.Devices.Roccat.RoccatDeviceProvider.PossibleX64NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x64", "RoccatTalkSDKWrapper.dll"));
            RGB.NET.Devices.Roccat.RoccatDeviceProvider.PossibleX86NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x86", "RoccatTalkSDKWrapper.dll"));
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }
    }
}