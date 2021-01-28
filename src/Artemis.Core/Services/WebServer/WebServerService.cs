using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using Ninject;
using Serilog;

namespace Artemis.Core.Services
{
    internal class WebServerService : IWebServerService, IDisposable
    {
        private readonly List<WebApiControllerRegistration> _controllers;
        private readonly IKernel _kernel;
        private readonly ILogger _logger;
        private readonly PluginsModule _pluginModule;
        private readonly PluginSetting<int> _webServerPortSetting;

        public WebServerService(IKernel kernel, ILogger logger, ISettingsService settingsService)
        {
            _kernel = kernel;
            _logger = logger;
            _controllers = new List<WebApiControllerRegistration>();

            _webServerPortSetting = settingsService.GetSetting("WebServer.Port", 9696);
            _webServerPortSetting.SettingChanged += WebServerPortSettingOnSettingChanged;

            _pluginModule = new PluginsModule("/plugin");
            Server = CreateWebServer();
            Server.Start();
        }

        #region Event handlers

        private void WebServerPortSettingOnSettingChanged(object? sender, EventArgs e)
        {
            Server = CreateWebServer();
            Server.Start();
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            Server?.Dispose();
            _webServerPortSetting.SettingChanged -= WebServerPortSettingOnSettingChanged;
        }

        #endregion

        public WebServer? Server { get; private set; }

        #region Web server managament

        private WebServer CreateWebServer()
        {
            Server?.Dispose();
            Server = null;

            string url = $"http://localhost:{_webServerPortSetting.Value}/";
            WebApiModule apiModule = new("/api/");
            WebServer server = new WebServer(o => o.WithUrlPrefix(url).WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithModule(apiModule)
                .WithModule(_pluginModule)
                .HandleHttpException((context, exception) => HandleHttpExceptionJson(context, exception));

            // Add controllers to the API module
            foreach (WebApiControllerRegistration registration in _controllers)
                apiModule.RegisterController(registration.ControllerType, (Func<WebApiController>) registration.UntypedFactory);

            // Listen for state changes.
            server.StateChanged += (s, e) => _logger.Verbose("WebServer new state - {state}", e.NewState);

            // Store the URL in a webserver.txt file so that remote applications can find it
            File.WriteAllText(Path.Combine(Constants.DataFolder, "webserver.txt"), url);
            OnWebServerCreated();

            return server;
        }

        private async Task HandleHttpExceptionJson(IHttpContext context, IHttpException httpException)
        {
            await context.SendStringAsync(JsonConvert.SerializeObject(httpException, Formatting.Indented), MimeType.Json, Encoding.UTF8);
        }

        #endregion

        #region Plugin endpoint management

        public JsonPluginEndPoint<T> AddJsonEndPoint<T>(PluginFeature feature, string endPointName, Action<T> requestHandler)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
            if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));
            JsonPluginEndPoint<T> endPoint = new(feature, endPointName, _pluginModule, requestHandler);
            _pluginModule.AddPluginEndPoint(endPoint);
            return endPoint;
        }

        public JsonPluginEndPoint<T> AddResponsiveJsonEndPoint<T>(PluginFeature feature, string endPointName, Func<T, object?> requestHandler)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
            if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));
            JsonPluginEndPoint<T> endPoint = new(feature, endPointName, _pluginModule, requestHandler);
            _pluginModule.AddPluginEndPoint(endPoint);
            return endPoint;
        }

        public StringPluginEndPoint AddStringEndPoint(PluginFeature feature, string endPointName, Action<string> requestHandler)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
            if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));
            StringPluginEndPoint endPoint = new(feature, endPointName, _pluginModule, requestHandler);
            _pluginModule.AddPluginEndPoint(endPoint);
            return endPoint;
        }

        public StringPluginEndPoint AddResponsiveStringEndPoint(PluginFeature feature, string endPointName, Func<string, string?> requestHandler)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
            if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));
            StringPluginEndPoint endPoint = new(feature, endPointName, _pluginModule, requestHandler);
            _pluginModule.AddPluginEndPoint(endPoint);
            return endPoint;
        }

        public RawPluginEndPoint AddRawEndPoint(PluginFeature feature, string endPointName, Action<IHttpContext> requestHandler)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
            if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));
            RawPluginEndPoint endPoint = new(feature, endPointName, _pluginModule, requestHandler);
            _pluginModule.AddPluginEndPoint(endPoint);
            return endPoint;
        }

        public void RemovePluginEndPoint(PluginEndPoint endPoint)
        {
            _pluginModule.RemovePluginEndPoint(endPoint);
        }

        #endregion

        #region Controller management

        public void AddController<T>() where T : WebApiController
        {
            _controllers.Add(new WebApiControllerRegistration<T>(_kernel));
            Server = CreateWebServer();
            Server.Start();
        }

        public void RemoveController<T>() where T : WebApiController
        {
            _controllers.RemoveAll(r => r.ControllerType == typeof(T));
            Server = CreateWebServer();
            Server.Start();
        }

        #endregion

        #region Events

        public event EventHandler? WebServerCreated;

        protected virtual void OnWebServerCreated()
        {
            WebServerCreated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}