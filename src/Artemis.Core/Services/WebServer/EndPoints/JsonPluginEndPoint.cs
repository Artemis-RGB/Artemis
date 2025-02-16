using System;
using System.Text.Json;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Conversion.Serializers.Json;

namespace Artemis.Core.Services;

/// <summary>
///     Represents a plugin web endpoint receiving an object of type <typeparamref name="T" /> and returning any
///     <see cref="object" /> or <see langword="null" />.
///     <para>Note: Both will be deserialized and serialized respectively using JSON.</para>
/// </summary>
public class JsonPluginEndPoint<T> : PluginEndPoint
{
    private readonly Action<T>? _requestHandler;
    private readonly Func<T, object?>? _responseRequestHandler;

    internal JsonPluginEndPoint(PluginFeature pluginFeature, string name, PluginsHandler pluginsHandler, Action<T> requestHandler) : base(pluginFeature, name, pluginsHandler)
    {
        _requestHandler = requestHandler;
        ThrowOnFail = true;
        Accepts = FlexibleContentType.Get(ContentType.ApplicationJson);
    }

    internal JsonPluginEndPoint(PluginFeature pluginFeature, string name, PluginsHandler pluginsHandler, Func<T, object?> responseRequestHandler) : base(pluginFeature, name, pluginsHandler)
    {
        _responseRequestHandler = responseRequestHandler;
        ThrowOnFail = true;
        Accepts = FlexibleContentType.Get(ContentType.ApplicationJson);
        Returns = FlexibleContentType.Get(ContentType.ApplicationJson);
    }

    /// <summary>
    ///     Whether or not the end point should throw an exception if deserializing the received JSON fails.
    ///     If set to <see langword="false" /> malformed JSON is silently ignored; if set to <see langword="true" /> malformed
    ///     JSON throws a <see cref="JsonException" />.
    /// </summary>
    public bool ThrowOnFail { get; set; }

    #region Overrides of PluginEndPoint

    /// <inheritdoc />
    protected override async Task<IResponse> ProcessRequest(IRequest request)
    {
        if (request.Method != RequestMethod.Post && request.Method != RequestMethod.Put)
            return request.Respond().Status(ResponseStatus.MethodNotAllowed).Build();

        if (request.Content == null)
            return request.Respond().Status(ResponseStatus.BadRequest).Build();

        object? response = null;
        try
        {
            T? deserialized = await JsonSerializer.DeserializeAsync<T>(request.Content, WebServerService.JsonOptions);
            if (deserialized == null)
                throw new JsonException("Deserialization returned null");

            if (_requestHandler != null)
            {
                _requestHandler(deserialized);
                return request.Respond().Status(ResponseStatus.NoContent).Build();
            }

            if (_responseRequestHandler != null)
                response = _responseRequestHandler(deserialized);
            else
                throw new ArtemisCoreException("JSON plugin end point has no request handler");
        }
        catch (JsonException)
        {
            if (ThrowOnFail)
                throw;
        }

        // TODO: Cache options
        if (response == null)
            return request.Respond().Status(ResponseStatus.NoContent).Build();
        return request.Respond()
            .Status(ResponseStatus.Ok)
            .Content(new JsonContent(response, WebServerService.JsonOptions))
            .Type(ContentType.ApplicationJson)
            .Build();
    }

    #endregion
}