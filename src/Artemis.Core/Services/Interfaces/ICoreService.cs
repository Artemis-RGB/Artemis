using System;
using Artemis.Core.Events;

namespace Artemis.Core.Services.Interfaces
{
    public interface ICoreService : IArtemisService, IDisposable
    {
        /// <summary>
        ///     Indicates wether or not the core has been initialized
        /// </summary>
        bool IsInitialized { get; set; }

        /// <summary>
        ///     Occurs the core has finished initializing
        /// </summary>
        event EventHandler Initialized;

        /// <summary>
        ///     Occurs whenever a frame has finished rendering
        /// </summary>
        event EventHandler<FrameEventArgs> FrameRendered;
    }
}