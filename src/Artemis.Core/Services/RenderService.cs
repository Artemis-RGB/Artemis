using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Artemis.Core.Providers;
using Artemis.Core.Services.Core;
using Artemis.Core.SkiaSharp;
using DryIoc;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;

namespace Artemis.Core.Services;

internal class RenderService : IRenderService, IRenderer, IDisposable
{
    private readonly Stopwatch _frameStopWatch;
    private readonly List<Exception> _updateExceptions = [];
    
    private readonly ILogger _logger;
    private readonly IDeviceService _deviceService;
    private readonly CoreRenderer _coreRenderer;
    private readonly LazyEnumerable<IGraphicsContextProvider> _graphicsContextProviders;
    private readonly PluginSetting<int> _targetFrameRateSetting;
    private readonly PluginSetting<double> _renderScaleSetting;
    private readonly PluginSetting<string> _preferredGraphicsContext;
    private readonly SurfaceManager _surfaceManager;
    
    private int _frames;
    private DateTime _lastExceptionLog;
    private DateTime _lastFrameRateSample;
    private bool _initialized;

    public RenderService(ILogger logger, ISettingsService settingsService, IDeviceService deviceService, CoreRenderer coreRenderer, LazyEnumerable<IGraphicsContextProvider> graphicsContextProviders)
    {
        _frameStopWatch = new Stopwatch();
        _logger = logger;
        _deviceService = deviceService;
        _coreRenderer = coreRenderer;
        _graphicsContextProviders = graphicsContextProviders;

        _targetFrameRateSetting = settingsService.GetSetting("Core.TargetFrameRate", 30);
        _renderScaleSetting = settingsService.GetSetting("Core.RenderScale", 0.5);
        _preferredGraphicsContext = settingsService.GetSetting("Core.PreferredGraphicsContext", "Software");
        _targetFrameRateSetting.SettingChanged += OnRenderSettingsChanged;
        _renderScaleSetting.SettingChanged += RenderScaleSettingOnSettingChanged;
        _preferredGraphicsContext.SettingChanged += PreferredGraphicsContextOnSettingChanged;

        Utilities.ShutdownRequested += UtilitiesOnShutdownRequested;
        _surfaceManager = new SurfaceManager(this, GraphicsContext, _targetFrameRateSetting.Value, (float) _renderScaleSetting.Value);
    }

    /// <inheritdoc />
    public IManagedGraphicsContext? GraphicsContext { get; private set; }

    /// <inheritdoc />
    public RGBSurface Surface => _surfaceManager.Surface;

    /// <inheritdoc />
    public bool IsPaused
    {
        get => _surfaceManager.IsPaused;
        set => _surfaceManager.SetPaused(value);
    }

    /// <inheritdoc />
    public int FrameRate { get; private set; }

    /// <inheritdoc />
    public TimeSpan FrameTime { get; private set; }

    /// <inheritdoc />
    public void Render(SKCanvas canvas, double delta)
    {
        _frameStopWatch.Restart();
        try
        {
            _coreRenderer.Render(canvas, delta);
            OnFrameRendering(new FrameRenderingEventArgs(canvas, delta, _surfaceManager.Surface));
        }
        catch (Exception e)
        {
            _updateExceptions.Add(e);
        }
    }

    /// <inheritdoc />
    public void PostRender(SKTexture texture)
    {
        try
        {
            _coreRenderer.PostRender(texture);
            OnFrameRendered(new FrameRenderedEventArgs(texture, _surfaceManager.Surface));
        }
        catch (Exception e)
        {
            _updateExceptions.Add(e);
        }
        finally
        {
            _frameStopWatch.Stop();
            _frames++;

            if ((DateTime.Now - _lastFrameRateSample).TotalSeconds >= 1)
            {
                FrameRate = _frames;
                _frames = 0;
                _lastFrameRateSample = DateTime.Now;
            }

            FrameTime = _frameStopWatch.Elapsed;

            LogUpdateExceptions();
        }
    }

    private void SetGraphicsContext()
    {
        if (Constants.StartupArguments.Contains("--force-software-render"))
        {
            _logger.Warning("Startup argument '--force-software-render' is applied, forcing software rendering");
            GraphicsContext = null;
            return;
        }

        _logger.Information("Applying {Name} graphics context", _preferredGraphicsContext.Value);
        if (_preferredGraphicsContext.Value == "Software")
        {
            GraphicsContext = null;
            return;
        }

        List<IGraphicsContextProvider> providers = _graphicsContextProviders.ToList();
        if (!providers.Any())
        {
            _logger.Warning("No graphics context provider found, defaulting to software rendering");
            GraphicsContext = null;
        }
        else
        {
            IManagedGraphicsContext? context = providers.FirstOrDefault(p => p.GraphicsContextName == _preferredGraphicsContext.Value)?.GetGraphicsContext();
            if (context == null)
                _logger.Warning("No graphics context named '{Context}' found, defaulting to software rendering", _preferredGraphicsContext.Value);

            GraphicsContext = context;
        }
    }
    
    private void LogUpdateExceptions()
    {
        // Only log update exceptions every 10 seconds to avoid spamming the logs
        if (DateTime.Now - _lastExceptionLog < TimeSpan.FromSeconds(10))
            return;
        _lastExceptionLog = DateTime.Now;

        if (!_updateExceptions.Any())
            return;

        // Group by stack trace, that should gather up duplicate exceptions
        foreach (IGrouping<string?, Exception> exceptions in _updateExceptions.GroupBy(e => e.StackTrace))
            _logger.Warning(exceptions.First(), "Exception was thrown {count} times during update in the last 10 seconds", exceptions.Count());

        // When logging is finished start with a fresh slate
        _updateExceptions.Clear();
    }

    private void DeviceServiceOnDeviceProviderAdded(object? sender, DeviceProviderEventArgs e)
    {
        _surfaceManager.AddDevices(e.Devices.Where(d => d.IsEnabled));
    }

    private void DeviceServiceOnDeviceProviderRemoved(object? sender, DeviceProviderEventArgs e)
    {
        _surfaceManager.RemoveDevices(e.Devices);
    }

    private void DeviceServiceOnDeviceEnabled(object? sender, DeviceEventArgs e)
    {
        _surfaceManager.AddDevices(new List<ArtemisDevice> {e.Device});
    }

    private void DeviceServiceOnDeviceDisabled(object? sender, DeviceEventArgs e)
    {
        _surfaceManager.RemoveDevices(new List<ArtemisDevice> {e.Device});
    }

    private void OnRenderSettingsChanged(object? sender, EventArgs e)
    {
        _surfaceManager.UpdateTargetFrameRate(_targetFrameRateSetting.Value);
    }

    private void RenderScaleSettingOnSettingChanged(object? sender, EventArgs e)
    {
        RenderScale.SetRenderScaleMultiplier((int) (1 / _renderScaleSetting.Value));
        _surfaceManager.UpdateRenderScale((float) _renderScaleSetting.Value);
    }

    private void PreferredGraphicsContextOnSettingChanged(object? sender, EventArgs e)
    {
        SetGraphicsContext();
        _surfaceManager.UpdateGraphicsContext(GraphicsContext);
    }

    private void UtilitiesOnShutdownRequested(object? sender, EventArgs e)
    {
        IsPaused = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        IsPaused = true;
        _surfaceManager.Dispose();
    }
    
    /// <inheritdoc />
    public event EventHandler<FrameRenderingEventArgs>? FrameRendering;
    
    /// <inheritdoc />
    public event EventHandler<FrameRenderedEventArgs>? FrameRendered;

    /// <inheritdoc />
    public void Initialize()
    {
        if (_initialized)
            return;
        
        SetGraphicsContext();
        _surfaceManager.AddDevices(_deviceService.EnabledDevices);
        
        _deviceService.DeviceProviderAdded += DeviceServiceOnDeviceProviderAdded;
        _deviceService.DeviceProviderRemoved += DeviceServiceOnDeviceProviderRemoved;
        _deviceService.DeviceEnabled += DeviceServiceOnDeviceEnabled;
        _deviceService.DeviceDisabled += DeviceServiceOnDeviceDisabled;
        
        IsPaused = false;
        _initialized = true;
    }

    protected virtual void OnFrameRendering(FrameRenderingEventArgs e)
    {
        FrameRendering?.Invoke(this, e);
    }

    protected virtual void OnFrameRendered(FrameRenderedEventArgs e)
    {
        FrameRendered?.Invoke(this, e);
    }
}