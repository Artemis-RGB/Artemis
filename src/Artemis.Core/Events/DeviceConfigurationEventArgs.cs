using System;
using Artemis.Core.Models.Surface;

namespace Artemis.Core.Events
{
    public class SurfaceConfigurationEventArgs : EventArgs
    {
        public SurfaceConfigurationEventArgs(SurfaceConfiguration surfaceConfiguration)
        {
            SurfaceConfiguration = surfaceConfiguration;
        }

        public SurfaceConfiguration SurfaceConfiguration { get; }
    }
}