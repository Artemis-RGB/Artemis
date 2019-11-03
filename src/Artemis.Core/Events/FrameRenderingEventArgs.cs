using System;
using System.Collections.Generic;
using System.Drawing;
using Artemis.Core.Plugins.Abstract;
using RGB.NET.Core;

namespace Artemis.Core.Events
{
    public class FrameRenderingEventArgs : EventArgs
    {
        public FrameRenderingEventArgs(List<Module> modules, Bitmap bitmap, double deltaTime, RGBSurface rgbSurface)
        {
            Modules = modules;
            Bitmap = bitmap;
            DeltaTime = deltaTime;
            RgbSurface = rgbSurface;
        }

        public List<Module> Modules { get; }
        public Bitmap Bitmap { get; }
        public double DeltaTime { get; }
        public RGBSurface RgbSurface { get; }
    }
}