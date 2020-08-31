using System.IO;
using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using RGB.NET.Core;
using RGB.NET.Devices.Razer;

namespace Artemis.Plugins.Devices.Razer
{
    // ReSharper disable once UnusedMember.Global
    public class RazerDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;

        public RazerDeviceProvider(IRgbService rgbService) : base(RGB.NET.Devices.Razer.RazerDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(RazerRGBDevice<>), sender, args);
            RGB.NET.Devices.Razer.RazerDeviceProvider.PossibleX64NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x64", "RzChromaSDK.dll"));
            RGB.NET.Devices.Razer.RazerDeviceProvider.PossibleX86NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x86", "RzChromaSDK.dll"));
            // RGB.NET.Devices.Razer.RazerDeviceProvider.Instance.LoadEmulatorDevices = true;

            try
            {
                _rgbService.AddDeviceProvider(RgbDeviceProvider);
            }
            catch (RazerException e)
            {
                throw new ArtemisPluginException("Failed to activate Razer plugin, error code: " + e.ErrorCode, e);
            }
        }
    }
}