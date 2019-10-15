using System;
using Artemis.Core.Models.Surface;
using RGB.NET.Core;

namespace Artemis.Core.Events
{
    public class SurfaceConfigurationEventArgs : EventArgs
    {
        public SurfaceConfigurationEventArgs(SurfaceConfiguration surfaceConfiguration, IRGBDevice device)
        {
            SurfaceConfiguration = surfaceConfiguration;
            Device = device;
        }

        public SurfaceConfiguration SurfaceConfiguration { get; }
        public IRGBDevice Device { get; }
    }
}