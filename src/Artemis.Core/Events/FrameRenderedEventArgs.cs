using System;
using RGB.NET.Core;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about frame rendering related events
    /// </summary>
    public class FrameRenderedEventArgs : EventArgs
    {
        internal FrameRenderedEventArgs(BitmapBrush bitmapBrush, RGBSurface rgbSurface)
        {
            BitmapBrush = bitmapBrush;
            RgbSurface = rgbSurface;
        }

        /// <summary>
        ///     Gets the bitmap brush used to render this frame
        /// </summary>
        public BitmapBrush BitmapBrush { get; }

        /// <summary>
        ///     Gets the RGB surface used to render this frame
        /// </summary>
        public RGBSurface RgbSurface { get; }
    }
}