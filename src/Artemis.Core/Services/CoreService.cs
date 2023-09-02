using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Artemis.Core.DryIoc.Factories;
using Artemis.Core.ScriptingProviders;
using Artemis.Storage;
using DryIoc;
using HidSharp;
using RGB.NET.Core;
using Serilog;
using Serilog.Events;
using SkiaSharp;

namespace Artemis.Core.Services;

/// <summary>
///     Provides Artemis's core update loop
/// </summary>
internal class CoreService : ICoreService
{
    private readonly Stopwatch _frameStopWatch;
    private readonly ILogger _logger;
    private readonly PluginSetting<LogEventLevel> _loggingLevel;
    private readonly IModuleService _moduleService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IProfileService _profileService;
    private readonly IRgbService _rgbService;
    private readonly IScriptingService _scriptingService;
    private readonly List<Exception> _updateExceptions = new();

    private int _frames;
    private DateTime _lastExceptionLog;
    private DateTime _lastFrameRateSample;

    // ReSharper disable UnusedParameter.Local
    public CoreService(IContainer container,
        ILogger logger,
        StorageMigrationService _1, // injected to ensure migration runs early
        ISettingsService settingsService,
        IPluginManagementService pluginManagementService,
        IRgbService rgbService,
        IProfileService profileService,
        IModuleService moduleService,
        IScriptingService scriptingService)
    {
        Constants.CorePlugin.Container = container;

        _logger = logger;
        _pluginManagementService = pluginManagementService;
        _rgbService = rgbService;
        _profileService = profileService;
        _moduleService = moduleService;
        _scriptingService = scriptingService;
        _loggingLevel = settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Debug);
        _frameStopWatch = new Stopwatch();

        _rgbService.Surface.Updating += SurfaceOnUpdating;
        _loggingLevel.SettingChanged += (sender, args) => ApplyLoggingLevel();
    }

    // ReSharper restore UnusedParameter.Local

    protected virtual void OnFrameRendering(FrameRenderingEventArgs e)
    {
        FrameRendering?.Invoke(this, e);
    }

    protected virtual void OnFrameRendered(FrameRenderedEventArgs e)
    {
        FrameRendered?.Invoke(this, e);
    }

    private void ApplyLoggingLevel()
    {
        string? argument = Constants.StartupArguments.FirstOrDefault(a => a.StartsWith("--logging"));
        if (argument != null)
        {
            // Parse the provided log level
            string[] parts = argument.Split('=');
            if (parts.Length == 2 && Enum.TryParse(typeof(LogEventLevel), parts[1], true, out object? logLevelArgument))
            {
                _logger.Information("Setting logging level to {loggingLevel} from startup argument", (LogEventLevel)logLevelArgument!);
                LoggerFactory.LoggingLevelSwitch.MinimumLevel = (LogEventLevel)logLevelArgument;
            }
            else
            {
                _logger.Warning("Failed to set log level from startup argument {argument}", argument);
                _logger.Information("Setting logging level to {loggingLevel}", _loggingLevel.Value);
                LoggerFactory.LoggingLevelSwitch.MinimumLevel = _loggingLevel.Value;
            }
        }
        else
        {
            _logger.Information("Setting logging level to {loggingLevel}", _loggingLevel.Value);
            LoggerFactory.LoggingLevelSwitch.MinimumLevel = _loggingLevel.Value;
        }
    }

    private void SurfaceOnUpdating(UpdatingEventArgs args)
    {
        if (_rgbService.IsRenderPaused)
            return;

        if (_rgbService.FlushLeds)
        {
            _rgbService.FlushLeds = false;
            _rgbService.Surface.Update(true);
            return;
        }

        try
        {
            _frameStopWatch.Restart();

            foreach (GlobalScript script in _scriptingService.GlobalScripts)
                script.OnCoreUpdating(args.DeltaTime);

            _moduleService.UpdateActiveModules(args.DeltaTime);
            SKTexture texture = _rgbService.OpenRender();
            SKCanvas canvas = texture.Surface.Canvas;
            canvas.Save();
            if (Math.Abs(texture.RenderScale - 1) > 0.001)
                canvas.Scale(texture.RenderScale);
            canvas.Clear(new SKColor(0, 0, 0));

            if (!ProfileRenderingDisabled)
            {
                _profileService.UpdateProfiles(args.DeltaTime);
                _profileService.RenderProfiles(canvas);
            }

            OnFrameRendering(new FrameRenderingEventArgs(canvas, args.DeltaTime, _rgbService.Surface));
            canvas.RestoreToCount(-1);
            canvas.Flush();

            OnFrameRendered(new FrameRenderedEventArgs(texture, _rgbService.Surface));

            foreach (GlobalScript script in _scriptingService.GlobalScripts)
                script.OnCoreUpdated(args.DeltaTime);
        }
        catch (Exception e)
        {
            _updateExceptions.Add(e);
        }
        finally
        {
            _rgbService.CloseRender();
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

    private void OnInitialized()
    {
        IsInitialized = true;
        Initialized?.Invoke(this, EventArgs.Empty);
    }

    public int FrameRate { get; private set; }
    public TimeSpan FrameTime { get; private set; }
    public bool ProfileRenderingDisabled { get; set; }
    public bool IsElevated { get; set; }

    public bool IsInitialized { get; set; }

    public void Initialize()
    {
        if (IsInitialized)
            throw new ArtemisCoreException("Cannot initialize the core as it is already initialized.");

        _logger.Information("Initializing Artemis Core version {CurrentVersion}", Constants.CurrentVersion);
        _logger.Information("Startup arguments: {StartupArguments}", Constants.StartupArguments);
        _logger.Information("Elevated permissions: {IsElevated}", IsElevated);
        _logger.Information("Stopwatch high resolution: {IsHighResolution}", Stopwatch.IsHighResolution);

        ApplyLoggingLevel();

        ProcessMonitor.Start();

        // Don't remove even if it looks useless
        // Just this line should prevent a certain someone from removing HidSharp as an unused dependency as well
        Version? hidSharpVersion = Assembly.GetAssembly(typeof(HidDevice))!.GetName().Version;
        _logger.Debug("Forcing plugins to use HidSharp {HidSharpVersion}", hidSharpVersion);

        // Initialize the services
        _pluginManagementService.CopyBuiltInPlugins();
        _pluginManagementService.LoadPlugins(IsElevated);

        _rgbService.ApplyPreferredGraphicsContext(Constants.StartupArguments.Contains("--force-software-render"));
        _rgbService.SetRenderPaused(false);
        OnInitialized();
    }

    public event EventHandler? Initialized;
    public event EventHandler<FrameRenderingEventArgs>? FrameRendering;
    public event EventHandler<FrameRenderedEventArgs>? FrameRendered;
}