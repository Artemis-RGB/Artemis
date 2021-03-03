using System;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides data about endpoint exception related events
    /// </summary>
    public class EndpointExceptionEventArgs : EventArgs
    {
        internal EndpointExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        ///     Gets the exception that occurred
        /// </summary>
        public Exception Exception { get; }
    }
}