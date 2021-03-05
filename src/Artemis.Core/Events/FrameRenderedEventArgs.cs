using System;
using RGB.NET.Core;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about frame rendering related events
    /// </summary>
    public class FrameRenderedEventArgs : EventArgs
    {
        internal FrameRenderedEventArgs(SKTexture texture, RGBSurface rgbSurface)
        {
            Texture = texture;
            RgbSurface = rgbSurface;
        }

        /// <summary>
        ///     Gets the texture used to render this frame
        /// </summary>
        public SKTexture Texture { get; }

        /// <summary>
        ///     Gets the RGB surface used to render this frame
        /// </summary>
        public RGBSurface RgbSurface { get; }
    }
}