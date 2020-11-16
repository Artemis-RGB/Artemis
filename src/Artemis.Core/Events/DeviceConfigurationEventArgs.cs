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

        /// <summary>
        ///     Gets the active surface at the time the event fired
        /// </summary>
        public ArtemisSurface Surface { get; }
    }
}