using System.IO;
using System.Linq;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using HidSharp;
using RGB.NET.Core;
using RGB.NET.Devices.Logitech;
using Serilog;
using Serilog.Events;

namespace Artemis.Plugins.Devices.Logitech
{
    public class LogitechDeviceProvider : DeviceProvider
    {
        private const int VENDOR_ID = 0x046D;
        private readonly ILogger _logger;
        private readonly IRgbService _rgbService;

        public LogitechDeviceProvider(PluginInfo pluginInfo, IRgbService rgbService, ILogger logger) : base(pluginInfo, RGB.NET.Devices.Logitech.LogitechDeviceProvider.Instance)
        {
            _rgbService = rgbService;
            _logger = logger;
        }

        public override void EnablePlugin()
        {
            PathHelper.ResolvingAbsolutePath += (sender, args) => ResolveAbsolutePath(typeof(LogitechRGBDevice<>), sender, args);
            RGB.NET.Devices.Logitech.LogitechDeviceProvider.PossibleX64NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x64", "LogitechLedEnginesWrapper.dll"));
            RGB.NET.Devices.Logitech.LogitechDeviceProvider.PossibleX86NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x86", "LogitechLedEnginesWrapper.dll"));
            _rgbService.AddDeviceProvider(RgbDeviceProvider);

            if (_logger.IsEnabled(LogEventLevel.Debug))
                LogDeviceIds();
        }
        
        private void LogDeviceIds()
        {
            var devices = DeviceList.Local.GetHidDevices(VENDOR_ID).DistinctBy(d => d.ProductID).ToList();
            _logger.Debug("Found {count} Logitech device(s)", devices.Count);
            foreach (var hidDevice in devices)
                _logger.Debug("Found Logitech device {name} with PID {pid}", hidDevice.GetFriendlyName(), hidDevice.ProductID);
        }
    }
}