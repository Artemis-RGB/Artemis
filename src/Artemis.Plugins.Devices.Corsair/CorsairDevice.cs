using System.IO;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using RGB.NET.Devices.Corsair;

namespace Artemis.Plugins.Devices.Corsair
{
    // ReSharper disable once UnusedMember.Global
    public class CorsairDevice : Device
    {
        private readonly IRgbService _rgbService;

        public CorsairDevice(PluginInfo pluginInfo, IRgbService rgbService) : base(pluginInfo, CorsairDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(CorsairRGBDevice<>), sender, args);
            CorsairDeviceProvider.PossibleX64NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x64", "CUESDK.x64_2017.dll"));
            CorsairDeviceProvider.PossibleX86NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x86", "CUESDK_2017.dll"));
            _rgbService.AddDeviceProvider(DeviceProvider);
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