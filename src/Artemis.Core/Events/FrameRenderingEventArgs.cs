using System;
using System.Collections.Generic;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.RGB.NET;
using RGB.NET.Core;

namespace Artemis.Core.Events
{
    public class FrameRenderingEventArgs : EventArgs
    {
        public FrameRenderingEventArgs(List<Module> modules, GraphicsDecorator graphicsDecorator, double deltaTime, RGBSurface rgbSurface)
        {
            Modules = modules;
            GraphicsDecorator = graphicsDecorator;
            DeltaTime = deltaTime;
            RgbSurface = rgbSurface;
        }

        public List<Module> Modules { get; }
        public GraphicsDecorator GraphicsDecorator { get; }
        public double DeltaTime { get; }
        public RGBSurface RgbSurface { get; }
    }
}