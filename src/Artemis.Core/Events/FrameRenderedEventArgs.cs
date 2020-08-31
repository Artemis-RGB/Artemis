using System;
using RGB.NET.Core;

namespace Artemis.Core
{
    public class FrameRenderedEventArgs : EventArgs
    {
        public FrameRenderedEventArgs(BitmapBrush bitmapBrush, RGBSurface rgbSurface)
        {
            BitmapBrush = bitmapBrush;
            RgbSurface = rgbSurface;
        }

        public BitmapBrush BitmapBrush { get; }
        public RGBSurface RgbSurface { get; }
    }
}