using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about surface configuration related events
    /// </summary>
    public class SurfaceConfigurationEventArgs : EventArgs
    {
        internal SurfaceConfigurationEventArgs(ArtemisSurface surface)
        {
            Surface = surface;
        }

        public ArtemisSurface Surface { get; }
    }
}