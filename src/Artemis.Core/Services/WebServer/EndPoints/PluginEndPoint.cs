using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace Artemis.Core.Services;

/// <summary>
///     Represents a base type for plugin end points to be targeted by the <see cref="PluginsModule" />
/// </summary>
public abstract class PluginEndPoint
{
    private readonly PluginsModule _pluginsModule;

    internal PluginEndPoint(PluginFeature pluginFeature, string name, PluginsModule pluginsModule)
    {
        _pluginsModule = pluginsModule;
        PluginFeature = pluginFeature;
        Name = name;

        PluginFeature.Disabled += OnDisabled;
    }

    /// <summary>
    ///     Gets the name of the end point
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the full URL of the end point
    /// </summary>
    public string Url => $"{_pluginsModule.ServerUrl?.TrimEnd('/')}{_pluginsModule.BaseRoute}{PluginFeature.Plugin.Guid}/{Name}";

    /// <summary>
    ///     Gets the plugin the end point is associated with
    /// </summary>
    [JsonIgnore]
    public PluginFeature PluginFeature { get; }

    /// <summary>
    ///     Gets the plugin info of the plugin the end point is associated with
    /// </summary>
    public PluginInfo PluginInfo => PluginFeature.Plugin.Info;

    /// <summary>
    ///     Gets the mime type of the input this end point accepts
    /// </summary>
    public ContentType Accepts { get; protected set; }

    /// <summary>
    ///     Gets the mime type of the output this end point returns
    /// </summary>
    public ContentType Returns { get; protected set; }

    /// <summary>
    ///     Occurs whenever a request threw an unhandled exception
    /// </summary>
    public event EventHandler<EndpointExceptionEventArgs>? RequestException;

    /// <summary>
    ///     Occurs whenever a request is about to be processed
    /// </summary>
    public event EventHandler<EndpointRequestEventArgs>? ProcessingRequest;

    /// <summary>
    ///     Occurs whenever a request was processed
    /// </summary>
    public event EventHandler<EndpointRequestEventArgs>? ProcessedRequest;

    /// <summary>
    ///     Called whenever the end point has to process a request
    /// </summary>
    /// <param name="request">The HTTP context of the request</param>
    protected abstract Task<IResponse> ProcessRequest(IRequest request);

    /// <summary>
    ///     Invokes the <see cref="RequestException" /> event
    /// </summary>
    /// <param name="e">The exception that occurred during the request</param>
    protected virtual void OnRequestException(Exception e)
    {
        RequestException?.Invoke(this, new EndpointExceptionEventArgs(e));
    }

    /// <summary>
    ///     Invokes the <see cref="ProcessingRequest" /> event
    /// </summary>
    protected virtual void OnProcessingRequest(IRequest request)
    {
        ProcessingRequest?.Invoke(this, new EndpointRequestEventArgs(request));
    }

    /// <summary>
    ///     Invokes the <see cref="ProcessedRequest" /> event
    /// </summary>
    protected virtual void OnProcessedRequest(IRequest request)
    {
        ProcessedRequest?.Invoke(this, new EndpointRequestEventArgs(request));
    }

    internal async Task<IResponse> InternalProcessRequest(IRequest context)
    {
        try
        {
            OnProcessingRequest(context);
            IResponse response = await ProcessRequest(context);
            OnProcessedRequest(context);
            return response;
        }
        catch (Exception e)
        {
            OnRequestException(e);
            return context.Respond()
                .Status(ResponseStatus.InternalServerError)
                .Content(new StringContent(e.ToString()))
                .Type(ContentType.TextPlain)
                .Build();
        }
    }

    private void OnDisabled(object? sender, EventArgs e)
    {
        PluginFeature.Disabled -= OnDisabled;
        _pluginsModule.RemovePluginEndPoint(this);
    }
}