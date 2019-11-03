using System;
using System.Drawing;
using RGB.NET.Core;

namespace Artemis.Core.Events
{
    public class FrameRenderedEventArgs : EventArgs
    {
        public FrameRenderedEventArgs(Bitmap bitmap, RGBSurface rgbSurface)
        {
            Bitmap = bitmap;
            RgbSurface = rgbSurface;
        }

        public Bitmap Bitmap { get; }
        public RGBSurface RgbSurface { get; }
    }
}