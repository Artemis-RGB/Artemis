using System;
using System.Threading.Tasks;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Modules;
using EmbedIO;
using EmbedIO.WebApi;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     A service that provides access to the local Artemis web server
    /// </summary>
    public interface IWebServerService : IArtemisService
    {
        /// <summary>
        ///     Gets the current instance of the web server, replaced when <see cref="WebServerStarting" /> occurs.
        /// </summary>
        WebServer? Server { get; }

        /// <summary>
        ///     Gets the plugins module containing all plugin end points
        /// </summary>
        PluginsModule PluginsModule { get; }

        /// <summary>
        ///     Adds a new endpoint for the given plugin feature receiving an object of type <typeparamref name="T" />
        ///     <para>Note: Object will be deserialized using JSON.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to be received</typeparam>
        /// <param name="feature">The plugin feature the end point is associated with</param>
        /// <param name="endPointName">The name of the end point, must be unique</param>
        /// <param name="requestHandler"></param>
        /// <returns>The resulting end point</returns>
        JsonPluginEndPoint<T> AddJsonEndPoint<T>(PluginFeature feature, string endPointName, Action<T> requestHandler);

        /// <summary>
        ///     Adds a new endpoint for the given plugin feature receiving an object of type <typeparamref name="T" /> and
        ///     returning any <see cref="object" />.
        ///     <para>Note: Both will be deserialized and serialized respectively using JSON.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to be received</typeparam>
        /// <param name="feature">The plugin feature the end point is associated with</param>
        /// <param name="endPointName">The name of the end point, must be unique</param>
        /// <param name="requestHandler"></param>
        /// <returns>The resulting end point</returns>
        JsonPluginEndPoint<T> AddResponsiveJsonEndPoint<T>(PluginFeature feature, string endPointName, Func<T, object?> requestHandler);

        /// <summary>
        ///     Adds a new endpoint that directly maps received JSON to the data model of the provided <paramref name="module" />.
        /// </summary>
        /// <typeparam name="T">The data model type of the module</typeparam>
        /// <param name="module">The module whose datamodel to apply the received JSON to</param>
        /// <param name="endPointName">The name of the end point, must be unique</param>
        /// <returns>The resulting end point</returns>
        DataModelJsonPluginEndPoint<T> AddDataModelJsonEndPoint<T>(Module<T> module, string endPointName) where T : DataModel;

        /// <summary>
        ///     Adds a new endpoint that directly maps received JSON to the data model of the provided <paramref name="profileModule" />.
        /// </summary>
        /// <typeparam name="T">The data model type of the module</typeparam>
        /// <param name="profileModule">The module whose datamodel to apply the received JSON to</param>
        /// <param name="endPointName">The name of the end point, must be unique</param>
        /// <returns>The resulting end point</returns>
        DataModelJsonPluginEndPoint<T> AddDataModelJsonEndPoint<T>(ProfileModule<T> profileModule, string endPointName) where T : DataModel;

        /// <summary>
        ///     Adds a new endpoint that directly maps received JSON to the data model of the provided <paramref name="dataModelExpansion" />.
        /// </summary>
        /// <typeparam name="T">The data model type of the module</typeparam>
        /// <param name="dataModelExpansion">The data model expansion whose datamodel to apply the received JSON to</param>
        /// <param name="endPointName">The name of the end point, must be unique</param>
        /// <returns>The resulting end point</returns>
        DataModelJsonPluginEndPoint<T> AddDataModelJsonEndPoint<T>(DataModelExpansion<T> dataModelExpansion, string endPointName) where T : DataModel;

        /// <summary>
        ///     Adds a new endpoint for the given plugin feature receiving an a <see cref="string" />.
        /// </summary>
        /// <param name="feature">The plugin feature the end point is associated with</param>
        /// <param name="endPointName">The name of the end point, must be unique</param>
        /// <param name="requestHandler"></param>
        /// <returns>The resulting end point</returns>
        StringPluginEndPoint AddStringEndPoint(PluginFeature feature, string endPointName, Action<string> requestHandler);

        /// <summary>
        ///     Adds a new endpoint for the given plugin feature receiving an a <see cref="string" /> and returning a
        ///     <see cref="string" /> or <see langword="null" />.
        /// </summary>
        /// <param name="feature">The plugin feature the end point is associated with</param>
        /// <param name="endPointName">The name of the end point, must be unique</param>
        /// <param name="requestHandler"></param>
        /// <returns>The resulting end point</returns>
        StringPluginEndPoint AddResponsiveStringEndPoint(PluginFeature feature, string endPointName, Func<string, string?> requestHandler);

        /// <summary>
        ///     Adds a new endpoint for the given plugin feature that handles a raw <see cref="IHttpContext" />.
        ///     <para>
        ///         Note: This requires that you reference the EmbedIO
        ///         <see href="https://www.nuget.org/packages/embedio">Nuget package</see>.
        ///     </para>
        /// </summary>
        /// <param name="feature">The plugin feature the end point is associated with</param>
        /// <param name="endPointName">The name of the end point, must be unique</param>
        /// <param name="requestHandler"></param>
        /// <returns>The resulting end point</returns>
        RawPluginEndPoint AddRawEndPoint(PluginFeature feature, string endPointName, Func<IHttpContext, Task> requestHandler);

        /// <summary>
        ///     Removes an existing endpoint
        /// </summary>
        /// <param name="endPoint">The end point to remove</param>
        void RemovePluginEndPoint(PluginEndPoint endPoint);

        /// <summary>
        ///     Adds a new Web API controller and restarts the web server
        /// </summary>
        /// <typeparam name="T">The type of Web API controller to remove</typeparam>
        void AddController<T>() where T : WebApiController;

        /// <summary>
        ///     Removes an existing Web API controller and restarts the web server
        /// </summary>
        /// <typeparam name="T">The type of Web API controller to remove</typeparam>
        void RemoveController<T>() where T : WebApiController;

        /// <summary>
        ///     Occurs when the web server has been created and is about to start. This is the ideal place to add your own modules.
        /// </summary>
        event EventHandler? WebServerStarting;
    }
}