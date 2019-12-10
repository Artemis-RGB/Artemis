using System;
using Artemis.Core.RGB.NET;
using RGB.NET.Core;

namespace Artemis.Core.Events
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