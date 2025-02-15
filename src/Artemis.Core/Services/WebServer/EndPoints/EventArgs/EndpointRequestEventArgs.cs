using System;
using GenHTTP.Api.Protocol;

namespace Artemis.Core.Services;

/// <summary>
///     Provides data about endpoint request related events
/// </summary>
public class EndpointRequestEventArgs : EventArgs
{
    internal EndpointRequestEventArgs(IRequest request)
    {
        Request = request;
    }

    /// <summary>
    ///     Gets the HTTP context of the request
    /// </summary>
    public IRequest Request { get; }
}