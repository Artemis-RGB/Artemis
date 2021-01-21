using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about application restart events
    /// </summary>
    public class RestartEventArgs : EventArgs
    {
        internal RestartEventArgs(bool elevate, TimeSpan delay)
        {
            Elevate = elevate;
            Delay = delay;
        }

        /// <summary>
        ///     Gets a boolean indicating whether the application should be restarted with elevated permissions
        /// </summary>
        public bool Elevate { get; }

        /// <summary>
        ///     Gets the delay before killing process and restarting
        /// </summary>
        public TimeSpan Delay { get; }
    }
}