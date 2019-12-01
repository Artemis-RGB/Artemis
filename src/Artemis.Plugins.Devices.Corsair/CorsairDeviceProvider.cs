using System.IO;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using RGB.NET.Devices.Corsair;

namespace Artemis.Plugins.Devices.Corsair
{
    // ReSharper disable once UnusedMember.Global
    public class CorsairDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;

        public CorsairDeviceProvider(PluginInfo pluginInfo, IRgbService rgbService) : base(pluginInfo, RGB.NET.Devices.Corsair.CorsairDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(CorsairRGBDevice<>), sender, args);
            RGB.NET.Devices.Corsair.CorsairDeviceProvider.PossibleX64NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x64", "CUESDK.x64_2017.dll"));
            RGB.NET.Devices.Corsair.CorsairDeviceProvider.PossibleX86NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x86", "CUESDK_2017.dll"));
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }

        public override void DisablePlugin()
        {
            // TODO: Remove the device provider from the surface
        }

        public override void Dispose()
        {
            // TODO: This will probably not go well without first removing the device provider
            // CorsairDeviceProvider.Instance.ResetDevices();
            // CorsairDeviceProvider.Instance.Dispose();
        }
    }
}