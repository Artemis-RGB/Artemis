using System;

namespace Artemis.Profiles.Layers.Types.AmbientLight.Model
{
    [Flags]
    public enum FlipMode
    {
        None = 0,
        Vertical = 1 << 0,
        Horizontal = 1 << 1
    }
}