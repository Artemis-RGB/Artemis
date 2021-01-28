using System;
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
        ///     Gets the currently active instance of the web server
        /// </summary>
        WebServer? Server { get; }

        /// <summary>
        ///     Adds a new endpoint for the given plugin feature
        /// </summary>
        /// <param name="feature">The plugin feature the end point is associated with</param>
        /// <param name="endPointName">The name of the end point, must be unique</param>
        /// <returns>The resulting end point</returns>
        PluginEndPoint AddPluginEndPoint(PluginFeature feature, string endPointName);

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
        ///     Occurs when a new instance of the web server was been created
        /// </summary>
        event EventHandler? WebServerCreated;
    }
}