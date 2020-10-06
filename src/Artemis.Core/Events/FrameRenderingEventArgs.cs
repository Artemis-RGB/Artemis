using System;
using System.Collections.Generic;
using Artemis.Core.Modules;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about frame rendered related events
    /// </summary>
    public class FrameRenderingEventArgs : EventArgs
    {
        internal FrameRenderingEventArgs(List<Module> modules, SKCanvas canvas, double deltaTime, RGBSurface rgbSurface)
        {
            Modules = modules;
            Canvas = canvas;
            DeltaTime = deltaTime;
            RgbSurface = rgbSurface;
        }

        public List<Module> Modules { get; }
        public SKCanvas Canvas { get; }
        public double DeltaTime { get; }
        public RGBSurface RgbSurface { get; }
    }
}