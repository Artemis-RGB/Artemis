using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core.Modules;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Security;
using Serilog;

namespace Artemis.Core.Services;

internal class WebServerService : IWebServerService, IDisposable
{
    private readonly List<WebApiControllerRegistration> _controllers;
    private readonly ILogger _logger;
    private readonly ICoreService _coreService;
    private readonly PluginSetting<bool> _webServerEnabledSetting;
    private readonly PluginSetting<int> _webServerPortSetting;
    private readonly SemaphoreSlim _webserverSemaphore = new(1, 1);
    internal static readonly JsonSerializerOptions JsonOptions = new(CoreJson.GetJsonSerializerOptions()) {ReferenceHandler = ReferenceHandler.IgnoreCycles, WriteIndented = true};

    public WebServerService(ILogger logger, ICoreService coreService, ISettingsService settingsService, IPluginManagementService pluginManagementService)
    {
        _logger = logger;
        _coreService = coreService;
        _controllers = new List<WebApiControllerRegistration>();

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
        _ = StartWebServer();
    }

    private void WebServerPortSettingOnSettingChanged(object? sender, EventArgs e)
    {
        _ = StartWebServer();
    }

    private void PluginManagementServiceOnPluginFeatureDisabled(object? sender, PluginFeatureEventArgs e)
    {
        bool mustRestart = false;
        if (_controllers.Any(c => c.Feature == e.PluginFeature))
        {
            mustRestart = true;
            _controllers.RemoveAll(c => c.Feature == e.PluginFeature);
        }
        
        if (mustRestart)
            _ = StartWebServer();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Server?.DisposeAsync();
        _webServerPortSetting.SettingChanged -= WebServerPortSettingOnSettingChanged;
    }

    public IServer? Server { get; private set; }
    public PluginsModule PluginsModule { get; }
    public event EventHandler? WebServerStarting;


    #region Web server managament

    private async Task <IServer> CreateWebServer()
    {
        if (Server != null)
        {
            await Server.DisposeAsync();
            OnWebServerStopped();
            Server = null;
        }

        PluginsModule.ServerUrl = $"http://localhost:{_webServerPortSetting.Value}/";

        LayoutBuilder serverLayout = Layout.Create()
            .Add(PluginsModule)
            .Add(ErrorHandler.Structured())
            .Add(CorsPolicy.Permissive());
       
        // Add registered controllers to the API module
        foreach (WebApiControllerRegistration registration in _controllers)
        {
            serverLayout = serverLayout.Add(registration.Path, Controller.From(registration.Factory()));
        }

        IServer server = Host.Create()
            .Handler(serverLayout.Build())
            .Bind(IPAddress.Loopback, (ushort) _webServerPortSetting.Value)
            .Development()
            .Build();
       
        // Store the URL in a webserver.txt file so that remote applications can find it
        await File.WriteAllTextAsync(Path.Combine(Constants.DataFolder, "webserver.txt"), PluginsModule.ServerUrl);

        return server;
    }

    private async Task StartWebServer()
    {
        await _webserverSemaphore.WaitAsync();
        try
        {
            // Don't create the webserver until after the core service is initialized, this avoids lots of useless re-creates during initialize
            if (!_coreService.IsInitialized)
                return;

            if (!_webServerEnabledSetting.Value)
                return;

            Server = await CreateWebServer();

            if (Constants.StartupArguments.Contains("--disable-webserver"))
            {
                _logger.Warning("Artemis launched with --disable-webserver, not enabling the webserver");
                return;
            }

            OnWebServerStarting();
            try
            {
                await Server.StartAsync();
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to start webserver");
                throw;
            }
            OnWebServerStarted();
        }
        finally
        {
            _webserverSemaphore.Release();
        }
    }

    private async Task AutoStartWebServer()
    {
        try
        {
            await StartWebServer();
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

    public RawPluginEndPoint AddRawEndPoint(PluginFeature feature, string endPointName, Func<IRequest, Task<IResponse>> requestHandler)
    {
        if (feature == null) throw new ArgumentNullException(nameof(feature));
        if (endPointName == null) throw new ArgumentNullException(nameof(endPointName));
        if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));
        RawPluginEndPoint endPoint = new(feature, endPointName, PluginsModule, requestHandler);
        PluginsModule.AddPluginEndPoint(endPoint);
        return endPoint;
    }

    [Obsolete("Use AddJsonEndPoint<T>(PluginFeature feature, string endPointName, Action<T> requestHandler) instead")]
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

    public WebApiControllerRegistration AddController<T>(PluginFeature feature, string path) where T : class
    {
        if (feature == null) throw new ArgumentNullException(nameof(feature));

        WebApiControllerRegistration<T> registration = new(this, feature, path);
        _controllers.Add(registration);
        _ = StartWebServer();

        return registration;
    }

    public void RemoveController(WebApiControllerRegistration registration)
    {
        _controllers.Remove(registration);
        _ = StartWebServer();
    }

    #endregion
}