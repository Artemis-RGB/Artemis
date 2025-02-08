using System;
using System.IO;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Strings;

namespace Artemis.Core.Services;

/// <summary>
///     Represents a plugin web endpoint receiving an a <see cref="string" /> and returning a <see cref="string" /> or
///     <see langword="null" />.
/// </summary>
public class StringPluginEndPoint : PluginEndPoint
{
    private readonly Action<string>? _requestHandler;
    private readonly Func<string, string?>? _responseRequestHandler;

    internal StringPluginEndPoint(PluginFeature pluginFeature, string name, PluginsModule pluginsModule, Action<string> requestHandler) : base(pluginFeature, name, pluginsModule)
    {
        _requestHandler = requestHandler;
        Accepts = ContentType.TextPlain;
    }

    internal StringPluginEndPoint(PluginFeature pluginFeature, string name, PluginsModule pluginsModule, Func<string, string?> requestHandler) : base(pluginFeature, name, pluginsModule)
    {
        _responseRequestHandler = requestHandler;
        Accepts = ContentType.TextPlain;
        Returns = ContentType.TextPlain;
    }

    #region Overrides of PluginEndPoint

    /// <inheritdoc />
    protected override async Task<IResponse> ProcessRequest(IRequest request)
    {
        if (request.Method != RequestMethod.Post && request.Method != RequestMethod.Put)
            return request.Respond().Status(ResponseStatus.MethodNotAllowed).Build();

        if (request.Content == null)
            return request.Respond().Status(ResponseStatus.BadRequest).Build();

        // Read the request as a string
        using StreamReader reader = new(request.Content);
        string? response;
        if (_requestHandler != null)
        {
            _requestHandler(await reader.ReadToEndAsync());
            return request.Respond().Status(ResponseStatus.Ok).Build();
        }

        if (_responseRequestHandler != null)
            response = _responseRequestHandler(await reader.ReadToEndAsync());
        else
            throw new ArtemisCoreException("String plugin end point has no request handler");

        if (response == null)
            return request.Respond().Status(ResponseStatus.NoContent).Build();
        return request.Respond()
            .Status(ResponseStatus.Ok)
            .Content(new StringContent(response))
            .Type(ContentType.TextPlain)
            .Build();
    }

    #endregion
}