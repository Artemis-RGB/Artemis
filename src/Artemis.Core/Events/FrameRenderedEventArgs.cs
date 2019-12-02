using System;
using System.Drawing;
using Artemis.Core.RGB.NET;
using RGB.NET.Core;

namespace Artemis.Core.Events
{
    public class FrameRenderedEventArgs : EventArgs
    {
        public FrameRenderedEventArgs(GraphicsDecorator graphicsDecorator, RGBSurface rgbSurface)
        {
            GraphicsDecorator = graphicsDecorator;
            RgbSurface = rgbSurface;
        }
        
        public GraphicsDecorator GraphicsDecorator { get; }
        public RGBSurface RgbSurface { get; }
    }
}