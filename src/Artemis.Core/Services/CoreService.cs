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
    private readonly ILogger _logger;
    private readonly PluginSetting<LogEventLevel> _loggingLevel;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IRenderService _renderService;


    // ReSharper disable UnusedParameter.Local
    public CoreService(IContainer container,
        ILogger logger,
        ISettingsService settingsService,
        IPluginManagementService pluginManagementService,
        IProfileService profileService,
        IModuleService moduleService,
        IScriptingService scriptingService,
        IRenderService renderService)
    {
        Constants.CorePlugin.Container = container;

        _logger = logger;
        _pluginManagementService = pluginManagementService;
        _renderService = renderService;
        _loggingLevel = settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Debug);
        _loggingLevel.SettingChanged += (sender, args) => ApplyLoggingLevel();
    }
   
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
        _renderService.Initialize();
        
        OnInitialized();
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

    public event EventHandler? Initialized;

    private void OnInitialized()
    {
        IsInitialized = true;
        Initialized?.Invoke(this, EventArgs.Empty);
    }
}