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

        public BitmapBrush BitmapBrush { get; }
        public RGBSurface RgbSurface { get; }
    }
}