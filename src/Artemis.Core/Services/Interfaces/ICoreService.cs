using System;
using Artemis.Core.Events;

namespace Artemis.Core.Services.Interfaces
{
    public interface ICoreService : IArtemisService, IDisposable
    {
        /// <summary>
        ///     Gets whether the or not the core has been initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets or sets whether modules are updated each frame by calling their Update method
        /// </summary>
        bool ModuleUpdatingDisabled { get; set; }
        /// <summary>
        /// Gets or sets whether modules are rendered each frame by calling their Render method
        /// </summary>
        bool ModuleRenderingDisabled { get; set; }

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