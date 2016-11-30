using System;

namespace Artemis.Profiles.Layers.Types.AmbientLight.Model
{
    [Flags]
    public enum BlackBarDetectionMode
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Top = 1 << 2,
        Bottom = 1 << 3
    }
}