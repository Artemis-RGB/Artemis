using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core.Modules;
using EmbedIO;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using Serilog;

namespace Artemis.Core.Services
{
    internal class WebServerService : IWebServerService, IDisposable
    {
        private readonly List<WebApiControllerRegistration> _controllers;
        private readonly List<WebModuleRegistration> _modules;
        private readonly ILogger _logger;
        private readonly PluginSetting<int> _webServerPortSetting;

        public WebServerService(ILogger logger, ISettingsService settingsService, IPluginManagementService pluginManagementService)
        {
            _logger = logger;
            _controllers = new List<WebApiControllerRegistration>();
            _modules = new List<WebModuleRegistration>();

            _webServerPortSetting = settingsService.GetSetting("WebServer.Port", 9696);
            _webServerPortSetting.SettingChanged += WebServerPortSettingOnSettingChanged;
            pluginManagementService.PluginFeatureDisabled += PluginManagementServiceOnPluginFeatureDisabled;

            PluginsModule = new PluginsModule("/plugins");
            StartWebServer();
        }

        public WebServer? Server { get; private set; }
        public PluginsModule PluginsModule { get; }

        #region Web server managament

        private WebServer CreateWebServer()
        {
            Server?.Dispose();
            Server = null;

            WebApiModule apiModule = new("/", JsonNetSerializer);
            PluginsModule.ServerUrl = $"http://localhost:{_webServerPortSetting.Value}/";
            WebServer server = new WebServer(o => o.WithUrlPrefix($"http://*:{_webServerPortSetting.Value}/").WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithModule(PluginsModule);

            // Add registered modules
            foreach (var webModule in _modules)
                server = server.WithModule(webModule.CreateInstance());

            server = server
                .WithModule(apiModule)
                .HandleHttpException((context, exception) => HandleHttpExceptionJson(context, exception))
                .HandleUnhandledException(JsonExceptionHandlerCallback);

            // Add registered controllers to the API module
            foreach (WebApiControllerRegistration registration in _controllers)
                apiModule.RegisterController(registration.ControllerType, (Func<WebApiController>) registration.UntypedFactory);

            // Listen for state changes.
            server.StateChanged += (s, e) => _logger.Verbose("WebServer new state - {state}", e.NewState);

            // Store the URL in a webserver.txt file so that remote applications can find it
            File.WriteAllText(Path.Combine(Constants.DataFolder, "webserver.txt"), PluginsModule.ServerUrl);

            return server;
        }

        private void StartWebServer()
        {
            Server = CreateWebServer();
            OnWebServerStarting();
            Server.Start();
        }

        #endregion

        #region Plugin endpoint management

        public JsonPluginEndPoint<T> AddJsonEndPoint<T>(PluginFeature feature, string endPointName, Action<T> requestHandler)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
            if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));
            JsonPluginEndPoint<T> endPoint = new(feature, endPointName, PluginsModule, requestHandler);
            PluginsModule.AddPluginEndPoint(endPoint);
            return endPoint;
        }

        public JsonPluginEndPoint<T> AddResponsiveJsonEndPoint<T>(PluginFeature feature, string endPointName, Func<T, object?> requestHandler)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
            if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));
            JsonPluginEndPoint<T> endPoint = new(feature, endPointName, PluginsModule, requestHandler);
            PluginsModule.AddPluginEndPoint(endPoint);
            return endPoint;
        }

        public StringPluginEndPoint AddStringEndPoint(PluginFeature feature, string endPointName, Action<string> requestHandler)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
            if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));
            StringPluginEndPoint endPoint = new(feature, endPointName, PluginsModule, requestHandler);
            PluginsModule.AddPluginEndPoint(endPoint);
            return endPoint;
        }

        public StringPluginEndPoint AddResponsiveStringEndPoint(PluginFeature feature, string endPointName, Func<string, string?> requestHandler)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
            if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));
            StringPluginEndPoint endPoint = new(feature, endPointName, PluginsModule, requestHandler);
            PluginsModule.AddPluginEndPoint(endPoint);
            return endPoint;
        }

        public RawPluginEndPoint AddRawEndPoint(PluginFeature feature, string endPointName, Func<IHttpContext, Task> requestHandler)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
            if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));
            RawPluginEndPoint endPoint = new(feature, endPointName, PluginsModule, requestHandler);
            PluginsModule.AddPluginEndPoint(endPoint);
            return endPoint;
        }

        public DataModelJsonPluginEndPoint<T> AddDataModelJsonEndPoint<T>(Module<T> module, string endPointName) where T : DataModel
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
            DataModelJsonPluginEndPoint<T> endPoint = new(module, endPointName, PluginsModule);
            PluginsModule.AddPluginEndPoint(endPoint);
            return endPoint;
        }

        private void HandleDataModelRequest<T>(Module<T> module, T value) where T : DataModel
        {
        }

        public void RemovePluginEndPoint(PluginEndPoint endPoint)
        {
            PluginsModule.RemovePluginEndPoint(endPoint);
        }

        #endregion

        #region Controller management

        public void AddController<T>(PluginFeature feature) where T : WebApiController
        {
            _controllers.Add(new WebApiControllerRegistration<T>(feature));
            StartWebServer();
        }

        public void RemoveController<T>() where T : WebApiController
        {
            _controllers.RemoveAll(r => r.ControllerType == typeof(T));
            StartWebServer();
        }

        #endregion

        #region Module management

        public void AddModule<T>(PluginFeature feature) where T : IWebModule
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (_modules.Any(r => r.WebModuleType == typeof(T)))
                return;

            _modules.Add(new WebModuleRegistration(feature, typeof(T)));
            StartWebServer();
        }

        public void RemoveModule<T>() where T : IWebModule
        {
            _modules.RemoveAll(r => r.WebModuleType == typeof(T));
            StartWebServer();
        }

        #endregion

        #region Handlers

        private async Task JsonExceptionHandlerCallback(IHttpContext context, Exception exception)
        {
            context.Response.ContentType = MimeType.Json;
            await using TextWriter writer = context.OpenResponseText();

            string response = JsonConvert.SerializeObject(new Dictionary<string, object?>()
            {
                {"StatusCode", context.Response.StatusCode},
                {"StackTrace", exception.StackTrace},
                {"Type", exception.GetType().FullName},
                {"Message", exception.Message},
                {"Data", exception.Data},
                {"InnerException", exception.InnerException},
                {"HelpLink", exception.HelpLink},
                {"Source", exception.Source},
                {"HResult", exception.HResult}
            });
            await writer.WriteAsync(response);
        }

        private async Task JsonNetSerializer(IHttpContext context, object? data)
        {
            context.Response.ContentType = MimeType.Json;
            await using TextWriter writer = context.OpenResponseText();
            string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings {PreserveReferencesHandling = PreserveReferencesHandling.Objects});
            await writer.WriteAsync(json);
        }

        private async Task HandleHttpExceptionJson(IHttpContext context, IHttpException httpException)
        {
            await context.SendStringAsync(JsonConvert.SerializeObject(httpException, Formatting.Indented), MimeType.Json, Encoding.UTF8);
        }

        #endregion

        #region Events

        public event EventHandler? WebServerStarting;

        protected virtual void OnWebServerStarting()
        {
            WebServerStarting?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Event handlers

        private void WebServerPortSettingOnSettingChanged(object? sender, EventArgs e)
        {
            StartWebServer();
        }

        private void PluginManagementServiceOnPluginFeatureDisabled(object? sender, PluginFeatureEventArgs e)
        {
            bool mustRestart = false;
            if (_controllers.Any(c => c.Feature == e.PluginFeature))
            {
                mustRestart = true;
                _controllers.RemoveAll(c => c.Feature == e.PluginFeature);
            }

            if (_modules.Any(m => m.Feature == e.PluginFeature))
            {
                mustRestart = true;
                _modules.RemoveAll(m => m.Feature == e.PluginFeature);
            }

            if (mustRestart)
                StartWebServer();
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
    }
}