using System;
using EmbedIO;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides data about endpoint request related events
    /// </summary>
    public class EndpointRequestEventArgs : EventArgs
    {
        internal EndpointRequestEventArgs(IHttpContext context)
        {
            Context = context;
        }

        /// <summary>
        ///     Gets the HTTP context of the request
        /// </summary>
        public IHttpContext Context { get; }
    }
}