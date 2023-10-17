using System;
using System.Collections.Generic;
using Artemis.Core.Services.Core;
using Artemis.Core.SkiaSharp;
using RGB.NET.Core;

namespace Artemis.Core.Services;

/// <summary>
///     Represents a service that manages the render loop and renderers.
/// </summary>
public interface IRenderService : IArtemisService
{
    /// <summary>
    ///     Gets the graphics context to be used for rendering
    /// </summary>
    IManagedGraphicsContext? GraphicsContext { get; }

    /// <summary>
    ///     Gets the RGB surface to which is being rendered.
    /// </summary>
    RGBSurface Surface { get; }

    /// <summary>
    ///     Gets or sets a boolean indicating whether rendering is paused.
    /// </summary>
    bool IsPaused { get; set; }
    
    /// <summary>
    ///     The time the last frame took to render
    /// </summary>
    TimeSpan FrameTime { get; }

    /// <summary>
    ///     The amount of frames rendered each second
    /// </summary>
    public int FrameRate { get; }
    
    /// <summary>
    ///     Initializes the render service and starts rendering.
    /// </summary>
    void Initialize();
    
    /// <summary>
    ///     Occurs whenever a frame is rendering, after modules have rendered
    /// </summary>
    event EventHandler<FrameRenderingEventArgs> FrameRendering;

    /// <summary>
    ///     Occurs whenever a frame is finished rendering and the render pipeline is closed
    /// </summary>
    event EventHandler<FrameRenderedEventArgs> FrameRendered;
}