using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace Artemis.Core.Services;

/// <summary>
///     Represents an GenHTTP handler used to process web requests and forward them to the right
///     <see cref="PluginEndPoint" />.
/// </summary>
public class StatusHandler : IHandler
{
    /// <inheritdoc />
    public ValueTask PrepareAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        // Used to be part of the RemoteController but moved here to avoid the /remote/ prefix enforced by GenHTTP
        return request.Target.Current?.Value == "status"
            ? ValueTask.FromResult<IResponse?>(request.Respond().Status(ResponseStatus.NoContent).Build())
            : ValueTask.FromResult<IResponse?>(null);
    }
}