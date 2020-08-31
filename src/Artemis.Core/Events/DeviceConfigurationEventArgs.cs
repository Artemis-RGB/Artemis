using System;

namespace Artemis.Core
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