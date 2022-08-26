using System;
using System.Collections.Generic;

namespace Artemis.Core.Services;

/// <summary>
///     A service that initializes the Core and manages the render loop
/// </summary>
public interface ICoreService : IArtemisService, IDisposable
{
    /// <summary>
    ///     Gets whether the or not the core has been initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    ///     The time the last frame took to render
    /// </summary>
    TimeSpan FrameTime { get; }

    /// <summary>
    ///     The amount of frames rendered each second
    /// </summary>
    public int FrameRate { get; }

    /// <summary>
    ///     Gets or sets whether profiles are rendered each frame by calling their Render method
    /// </summary>
    bool ProfileRenderingDisabled { get; set; }
    
    /// <summary>
    ///     Gets a boolean indicating whether Artemis is running in an elevated environment (admin permissions)
    /// </summary>
    bool IsElevated { get; set; }

    /// <summary>
    ///     Initializes the core, only call once
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Occurs the core has finished initializing
    /// </summary>
    event EventHandler Initialized;

    /// <summary>
    ///     Occurs whenever a frame is rendering, after modules have rendered
    /// </summary>
    event EventHandler<FrameRenderingEventArgs> FrameRendering;

    /// <summary>
    ///     Occurs whenever a frame is finished rendering and the render pipeline is closed
    /// </summary>
    event EventHandler<FrameRenderedEventArgs> FrameRendered;
}