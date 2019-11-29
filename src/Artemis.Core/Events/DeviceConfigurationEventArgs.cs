using System;
using Artemis.Core.Models.Surface;

namespace Artemis.Core.Events
{
    public class SurfaceConfigurationEventArgs : EventArgs
    {
        public SurfaceConfigurationEventArgs(ArtemisSurface surface)
        {
            Surface = surface;
        }

        public ArtemisSurface Surface { get; }
    }
}