using System;
using System.IO;
using System.Threading.Tasks;
using EmbedIO;

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
        Accepts = MimeType.PlainText;
    }

    internal StringPluginEndPoint(PluginFeature pluginFeature, string name, PluginsModule pluginsModule, Func<string, string?> requestHandler) : base(pluginFeature, name, pluginsModule)
    {
        _responseRequestHandler = requestHandler;
        Accepts = MimeType.PlainText;
        Returns = MimeType.PlainText;
    }

    #region Overrides of PluginEndPoint

    /// <inheritdoc />
    protected override async Task ProcessRequest(IHttpContext context)
    {
        if (context.Request.HttpVerb != HttpVerbs.Post && context.Request.HttpVerb != HttpVerbs.Put)
            throw HttpException.MethodNotAllowed("This end point only accepts POST and PUT calls");

        context.Response.ContentType = MimeType.PlainText;

        using TextReader reader = context.OpenRequestText();
        string? response;
        if (_requestHandler != null)
        {
            _requestHandler(await reader.ReadToEndAsync());
            return;
        }

        if (_responseRequestHandler != null)
            response = _responseRequestHandler(await reader.ReadToEndAsync());
        else
            throw new ArtemisCoreException("String plugin end point has no request handler");

        await using TextWriter writer = context.OpenResponseText();
        await writer.WriteAsync(response);
    }

    #endregion
}