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