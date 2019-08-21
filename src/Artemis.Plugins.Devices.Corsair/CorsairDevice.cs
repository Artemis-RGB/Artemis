using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Artemis.Core.Extensions;
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

        public CorsairDevice(PluginInfo pluginInfo, IRgbService rgbService) : base(pluginInfo)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            PathHelper.ResolvingAbsolutePath += ResolveCorsairPath;   
            CorsairDeviceProvider.PossibleX64NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x64", "CUESDK.dll"));
            CorsairDeviceProvider.PossibleX86NativePaths.Add(Path.Combine(PluginInfo.Directory.FullName, "x86", "CUESDK.dll"));
            _rgbService.AddDeviceProvider(CorsairDeviceProvider.Instance);
        }

        private void ResolveCorsairPath(object sender, ResolvePathEventArgs e)
        {
            if (sender.GetType().IsGenericType(typeof(CorsairRGBDevice<>)))
            {
                // Start from the plugin directory
                e.FinalPath = Path.Combine(PluginInfo.Directory.FullName, e.RelativePart, e.FileName);
            }
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