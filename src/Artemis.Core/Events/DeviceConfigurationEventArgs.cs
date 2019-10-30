using System;
using Artemis.Core.Models.Surface;

namespace Artemis.Core.Events
{
    public class SurfaceConfigurationEventArgs : EventArgs
    {
        public SurfaceConfigurationEventArgs(Surface surface)
        {
            Surface = surface;
        }

        public Surface Surface { get; }
    }
}