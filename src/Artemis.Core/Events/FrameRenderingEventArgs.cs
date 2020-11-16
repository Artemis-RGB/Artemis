using System;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about frame rendered related events
    /// </summary>
    public class FrameRenderingEventArgs : EventArgs
    {
        internal FrameRenderingEventArgs(SKCanvas canvas, double deltaTime, RGBSurface rgbSurface)
        {
            Canvas = canvas;
            DeltaTime = deltaTime;
            RgbSurface = rgbSurface;
        }

        /// <summary>
        ///     Gets the canvas this frame is rendering on
        /// </summary>
        public SKCanvas Canvas { get; }

        /// <summary>
        ///     Gets the delta time since the last frame was rendered
        /// </summary>
        public double DeltaTime { get; }

        /// <summary>
        ///     Gets the RGB surface used to render this frame
        /// </summary>
        public RGBSurface RgbSurface { get; }
    }
}