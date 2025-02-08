using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Conversion.Serializers.Json;
using GenHTTP.Modules.IO.Strings;

namespace Artemis.Core.Services;

/// <summary>
///     Represents an EmbedIO web module used to process web requests and forward them to the right
///     <see cref="PluginEndPoint" />.
/// </summary>
public class PluginsModule : IHandler
{
    private readonly Dictionary<string, Dictionary<string, PluginEndPoint>> _pluginEndPoints;

    internal PluginsModule(string baseRoute)
    {
        BaseRoute = baseRoute;
        _pluginEndPoints = new Dictionary<string, Dictionary<string, PluginEndPoint>>(comparer: StringComparer.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Gets the base route of the module
    /// </summary>
    public string BaseRoute { get; }

    internal void AddPluginEndPoint(PluginEndPoint registration)
    {
        string id = registration.PluginFeature.Plugin.Guid.ToString();
        if (!_pluginEndPoints.TryGetValue(id, out Dictionary<string, PluginEndPoint>? registrations))
        {
            registrations = new Dictionary<string, PluginEndPoint>();
            _pluginEndPoints.Add(id, registrations);
        }

        if (registrations.ContainsKey(registration.Name))
            throw new ArtemisPluginException(registration.PluginFeature.Plugin, $"Plugin already registered an endpoint at {registration.Name}.");
        registrations.Add(registration.Name, registration);
    }

    internal void RemovePluginEndPoint(PluginEndPoint registration)
    {
        string id = registration.PluginFeature.Plugin.Guid.ToString();
        if (!_pluginEndPoints.TryGetValue(id, out Dictionary<string, PluginEndPoint>? registrations))
            return;
        if (!registrations.ContainsKey(registration.Name))
            return;
        registrations.Remove(registration.Name);
    }
    
    /// <inheritdoc />
    public ValueTask PrepareAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        // Expect a plugin ID and an endpoint
        if (request.Target.Path.Parts.Count != 2)
            return null;
        
        // Find a matching plugin, if none found let another handler have a go :)
        if (!_pluginEndPoints.TryGetValue(request.Target.Path.Parts[0].Value, out Dictionary<string, PluginEndPoint>? endPoints))
            return null;

        // Find a matching endpoint
        if (!endPoints.TryGetValue(request.Target.Path.Parts[1].Value, out PluginEndPoint? endPoint))
        {
            return request.Respond()
                .Status(ResponseStatus.NotFound)
                .Content(new StringContent($"Found no endpoint called {request.Target.Path.Parts[1].Value} for plugin with ID {request.Target.Path.Parts[0].Value}."))
                .Type(ContentType.TextPlain)
                .Build();
        }

        // It is up to the registration how the request is eventually handled
        return await endPoint.InternalProcessRequest(request);
    }

    #region Overrides of WebModuleBase

    internal string? ServerUrl { get; set; }

    /// <summary>
    ///     Gets a read only collection containing all current plugin end points
    /// </summary>
    public IReadOnlyCollection<PluginEndPoint> PluginEndPoints => new List<PluginEndPoint>(_pluginEndPoints.SelectMany(p => p.Value.Values));

    #endregion
}