using System;
using System.IO;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using RGB.NET.Devices.Logitech;

namespace Artemis.Plugins.Devices.Logitech
{
    public class LogitechDevice : Device
    {
        private readonly IRgbService _rgbService;

        public LogitechDevice(PluginInfo pluginInfo, IRgbService rgbService) : base(pluginInfo, LogitechDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(LogitechRGBDevice<>), sender, args);
            LogitechDeviceProvider.PossibleX64NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x64", "LogitechLedEnginesWrapper.dll"));
            LogitechDeviceProvider.PossibleX86NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x86", "LogitechLedEnginesWrapper.dll"));
            _rgbService.AddDeviceProvider(DeviceProvider);
        }
        
        public override void DisablePlugin()
        {
        }

        public override void Dispose()
        {
        }
    }
}