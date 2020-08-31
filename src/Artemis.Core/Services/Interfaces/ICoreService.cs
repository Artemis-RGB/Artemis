using System;
using System.Collections.Generic;

namespace Artemis.Core.Services
{
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
        ///     Gets or sets whether modules are rendered each frame by calling their Render method
        /// </summary>
        bool ModuleRenderingDisabled { get; set; }

        /// <summary>
        ///     Gets or sets a list of startup arguments
        /// </summary>
        List<string> StartupArguments { get; set; }

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
        ///     Occurs whenever a frame is finished rendering and processed by RGB.NET
        /// </summary>
        event EventHandler<FrameRenderedEventArgs> FrameRendered;
    }
}