using System;
using Artemis.Core.Events;

namespace Artemis.Core.Services.Interfaces
{
    public interface ICoreService: IArtemisService, IDisposable
    {
        bool IsInitialized { get; set; }

        /// <summary>
        ///     Occurs the core has finished initializing
        /// </summary>
        event EventHandler Initialized;
    }
}