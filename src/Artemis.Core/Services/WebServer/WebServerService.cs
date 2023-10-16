using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core.Modules;
using EmbedIO;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using Serilog;

namespace Artemis.Core.Services;

internal class WebServerService : IWebServerService, IDisposable
{
    private readonly List<WebApiControllerRegistration> _controllers;
    private readonly ILogger _logger;
    private readonly ICoreService _coreService;
    private readonly List<WebModuleRegistration> _modules;
    private readonly PluginSetting<bool> _webServerEnabledSetting;
    private readonly PluginSetting<int> _webServerPortSetting;
    private readonly object _webserverLock = new();
    private CancellationTokenSource? _cts;

    public WebServerService(ILogger logger, ICoreService coreService, ISettingsService settingsService, IPluginManagementService pluginManagementService)
    {
        _logger = logger;
        _coreService = coreService;
        _controllers = new List<WebApiControllerRegistration>();
        _modules = new List<WebModuleRegistration>();

        _webServerEnabledSetting = settingsService.GetSetting("WebServer.Enabled", true);
        _webServerPortSetting = settingsService.GetSetting("WebServer.Port", 9696);
        _webServerEnabledSetting.SettingChanged += WebServerEnabledSettingOnSettingChanged;
        _webServerPortSetting.SettingChanged += WebServerPortSettingOnSettingChanged;
        pluginManagementService.PluginFeatureDisabled += PluginManagementServiceOnPluginFeatureDisabled;

        PluginsModule = new PluginsModule("/plugins");
        if (coreService.IsInitialized)
            AutoStartWebServer();
        else
            coreService.Initialized += (sender, args) => AutoStartWebServer();
    }

    public event EventHandler? WebServerStopped;
    public event EventHandler? WebServerStarted;

    protected virtual void OnWebServerStopped()
    {
        WebServerStopped?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnWebServerStarting()
    {
        WebServerStarting?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnWebServerStarted()
    {
        WebServerStarted?.Invoke(this, EventArgs.Empty);
    }

    private void WebServerEnabledSettingOnSettingChanged(object? sender, EventArgs e)
    {
        StartWebServer();
    }

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

    /// <inheritdoc />
    public void Dispose()
    {
        Server?.Dispose();
        _webServerPortSetting.SettingChanged -= WebServerPortSettingOnSettingChanged;
    }

    public WebServer? Server { get; private set; }
    public PluginsModule PluginsModule { get; }
    public event EventHandler? WebServerStarting;


    #region Web server managament

    private WebServer CreateWebServer()
    {
        if (Server != null)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
            }

            Server.Dispose();
            OnWebServerStopped();
            Server = null;
        }

        WebApiModule apiModule = new("/", JsonNetSerializer);
        PluginsModule.ServerUrl = $"http://localhost:{_webServerPortSetting.Value}/";
        WebServer server = new WebServer(o => o.WithUrlPrefix($"http://*:{_webServerPortSetting.Value}/").WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager()
            .WithModule(PluginsModule);

        // Add registered modules
        foreach (WebModuleRegistration? webModule in _modules)
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
        lock (_webserverLock)
        {
            // Don't create the webserver until after the core service is initialized, this avoids lots of useless re-creates during initialize
            if (!_coreService.IsInitialized)
                return;

            if (!_webServerEnabledSetting.Value)
                return;

            Server = CreateWebServer();

            if (Constants.StartupArguments.Contains("--disable-webserver"))
            {
                _logger.Warning("Artemis launched with --disable-webserver, not enabling the webserver");
                return;
            }

            OnWebServerStarting();
            _cts = new CancellationTokenSource();
            Server.Start(_cts.Token);
            OnWebServerStarted();
        }
    }
    
    private void AutoStartWebServer()
    {
        try
        {
            StartWebServer();
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "Failed to initially start webserver");
        }
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

    public DataModelJsonPluginEndPoint<T> AddDataModelJsonEndPoint<T>(Module<T> module, string endPointName) where T : DataModel, new()
    {
        if (module == null) throw new ArgumentNullException(nameof(module));
        if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
        DataModelJsonPluginEndPoint<T> endPoint = new(module, endPointName, PluginsModule);
        PluginsModule.AddPluginEndPoint(endPoint);
        return endPoint;
    }

    public void RemovePluginEndPoint(PluginEndPoint endPoint)
    {
        PluginsModule.RemovePluginEndPoint(endPoint);
    }

    #endregion

    #region Controller management

    public WebApiControllerRegistration AddController<T>(PluginFeature feature) where T : WebApiController
    {
        if (feature == null) throw new ArgumentNullException(nameof(feature));

        WebApiControllerRegistration<T> registration = new(this, feature);
        _controllers.Add(registration);
        StartWebServer();

        return registration;
    }

    public void RemoveController(WebApiControllerRegistration registration)
    {
        _controllers.Remove(registration);
        StartWebServer();
    }

    #endregion

    #region Module management

    public WebModuleRegistration AddModule(PluginFeature feature, Func<IWebModule> create)
    {
        if (feature == null) throw new ArgumentNullException(nameof(feature));

        WebModuleRegistration registration = new(this, feature, create);
        _modules.Add(registration);
        StartWebServer();

        return registration;
    }

    public WebModuleRegistration AddModule<T>(PluginFeature feature) where T : IWebModule
    {
        if (feature == null) throw new ArgumentNullException(nameof(feature));

        WebModuleRegistration registration = new(this, feature, typeof(T));
        _modules.Add(registration);
        StartWebServer();

        return registration;
    }

    public void RemoveModule(WebModuleRegistration registration)
    {
        _modules.Remove(registration);
        StartWebServer();
    }

    #endregion

    #region Handlers

    private async Task JsonExceptionHandlerCallback(IHttpContext context, Exception exception)
    {
        context.Response.ContentType = MimeType.Json;
        await using TextWriter writer = context.OpenResponseText();

        string response = JsonConvert.SerializeObject(new Dictionary<string, object?>
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
}