using System;
using System.Threading.Tasks;
using EmbedIO;

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
    internal RawPluginEndPoint(PluginFeature pluginFeature, string name, PluginsModule pluginsModule, Func<IHttpContext, Task> requestHandler) : base(pluginFeature, name, pluginsModule)
    {
        RequestHandler = requestHandler;
    }

    /// <summary>
    ///     Gets or sets the handler used to handle incoming requests to this endpoint
    /// </summary>
    public Func<IHttpContext, Task> RequestHandler { get; }

    /// <summary>
    ///     Sets the mime type this plugin end point accepts
    /// </summary>
    public void SetAcceptType(string type)
    {
        Accepts = type;
    }

    /// <summary>
    ///     Sets the mime type this plugin end point returns
    /// </summary>
    public void SetReturnType(string type)
    {
        Returns = type;
    }

    #region Overrides of PluginEndPoint

    /// <inheritdoc />
    protected override async Task ProcessRequest(IHttpContext context)
    {
        await RequestHandler(context);
    }

    #endregion
}