using System;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;

namespace Artemis.Core.Services;

/// <summary>
///     Represents a plugin web endpoint that handles a raw <see cref="IHttpContext" />.
///     <para>
///         Note: This requires that you reference the EmbedIO
///         <see href="https://www.nuget.org/packages/embedio">Nuget package</see>.
///     </para>
/// </summary>
public class RawPluginEndPoint : PluginEndPoint
{
    /// <inheritdoc />
    internal RawPluginEndPoint(PluginFeature pluginFeature, string name, PluginsHandler pluginsHandler, Func<IRequest, Task<IResponse>> requestHandler) : base(pluginFeature, name, pluginsHandler)
    {
        RequestHandler = requestHandler;
    }

    /// <summary>
    ///     Gets or sets the handler used to handle incoming requests to this endpoint
    /// </summary>
    public Func<IRequest, Task<IResponse>> RequestHandler { get; }

    /// <summary>
    ///     Sets the mime type this plugin end point accepts
    /// </summary>
    public void SetAcceptType(ContentType type)
    {
        Accepts = FlexibleContentType.Get(type);
    }

    /// <summary>
    ///     Sets the mime type this plugin end point returns
    /// </summary>
    public void SetReturnType(ContentType type)
    {
        Returns = FlexibleContentType.Get(type);
    }

    #region Overrides of PluginEndPoint

    /// <inheritdoc />
    protected override async Task<IResponse> ProcessRequest(IRequest request)
    {
        return await RequestHandler(request);
    }

    #endregion
}