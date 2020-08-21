using System.IO;
using Artemis.Core.Plugins.DeviceProviders;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using RGB.NET.Devices.CoolerMaster;

namespace Artemis.Plugins.Devices.CoolerMaster
{
    // ReSharper disable once UnusedMember.Global
    public class CoolerMasterDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;

        public CoolerMasterDeviceProvider(IRgbService rgbService) : base(RGB.NET.Devices.CoolerMaster.CoolerMasterDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(CoolerMasterRGBDevice<>), sender, args);
            RGB.NET.Devices.CoolerMaster.CoolerMasterDeviceProvider.PossibleX64NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x64", "CMSDK.dll"));
            RGB.NET.Devices.CoolerMaster.CoolerMasterDeviceProvider.PossibleX86NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x86", "CMSDK.dll"));
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }
    }
}