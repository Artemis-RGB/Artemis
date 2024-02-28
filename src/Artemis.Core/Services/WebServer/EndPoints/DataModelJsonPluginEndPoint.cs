using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Artemis.Core.Modules;
using EmbedIO;

namespace Artemis.Core.Services;

/// <summary>
///     Represents a plugin web endpoint receiving an object of type <typeparamref name="T" /> and returning any
///     <see cref="object" /> or <see langword="null" />.
///     <para>Note: Both will be deserialized and serialized respectively using JSON.</para>
/// </summary>
public class DataModelJsonPluginEndPoint<T> : PluginEndPoint where T : DataModel, new()
{
    private readonly Module<T> _module;
    private readonly Action<T, T> _update;

    internal DataModelJsonPluginEndPoint(Module<T> module, string name, PluginsModule pluginsModule) : base(module, name, pluginsModule)
    {
        _module = module ?? throw new ArgumentNullException(nameof(module));
        _update = CreateUpdateAction();

        ThrowOnFail = true;
        Accepts = MimeType.Json;
    }

    /// <summary>
    ///     Whether or not the end point should throw an exception if deserializing the received JSON fails.
    ///     If set to <see langword="false" /> malformed JSON is silently ignored; if set to <see langword="true" /> malformed
    ///     JSON throws a <see cref="JsonException" />.
    /// </summary>
    public bool ThrowOnFail { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRequest(IHttpContext context)
    {
        if (context.Request.HttpVerb != HttpVerbs.Post && context.Request.HttpVerb != HttpVerbs.Put)
            throw HttpException.MethodNotAllowed("This end point only accepts POST and PUT calls");

        context.Response.ContentType = MimeType.Json;

        using TextReader reader = context.OpenRequestText();
        try
        {
            T? dataModel = CoreJson.Deserialize<T>(await reader.ReadToEndAsync());
            if (dataModel != null)
                _update(dataModel, _module.DataModel);
        }
        catch (JsonException)
        {
            if (ThrowOnFail)
                throw;
        }
    }

    private Action<T, T> CreateUpdateAction()
    {
        ParameterExpression sourceParameter = Expression.Parameter(typeof(T), "source");
        ParameterExpression targetParameter = Expression.Parameter(typeof(T), "target");

        IEnumerable<BinaryExpression> assignments = typeof(T)
            .GetProperties()
            .Where(prop => prop.CanWrite && prop.GetSetMethod() != null &&
                           prop.GetSetMethod()!.IsPublic &&
                           !prop.IsDefined(typeof(JsonIgnoreAttribute), false) &&
                           !prop.PropertyType.IsAssignableTo(typeof(IDataModelEvent)))
            .Select(prop =>
            {
                MemberExpression sourceProperty = Expression.Property(sourceParameter, prop);
                MemberExpression targetProperty = Expression.Property(targetParameter, prop);
                return Expression.Assign(targetProperty, sourceProperty);
            });

        BlockExpression body = Expression.Block(assignments);

        return Expression.Lambda<Action<T, T>>(body, sourceParameter, targetParameter).Compile();
    }
}