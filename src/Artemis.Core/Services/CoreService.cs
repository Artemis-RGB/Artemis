using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core.Ninject;
using Artemis.Storage;
using HidSharp;
using Ninject;
using RGB.NET.Core;
using Serilog;
using Serilog.Events;
using SkiaSharp;
using Module = Artemis.Core.Modules.Module;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides Artemis's core update loop
    /// </summary>
    internal class CoreService : ICoreService
    {
        internal static IKernel? Kernel;

        private readonly Stopwatch _frameStopWatch;
        private readonly ILogger _logger;
        private readonly PluginSetting<LogEventLevel> _loggingLevel;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IProfileService _profileService;
        private readonly IModuleService _moduleService;
        private readonly IRgbService _rgbService;
        private readonly List<Exception> _updateExceptions = new();
        private DateTime _lastExceptionLog;

        // ReSharper disable UnusedParameter.Local
        public CoreService(IKernel kernel,
            ILogger logger,
            StorageMigrationService _, // injected to ensure migration runs early
            ISettingsService settingsService,
            IPluginManagementService pluginManagementService,
            IRgbService rgbService,
            IProfileService profileService,
            IModuleService moduleService)
        {
            Kernel = kernel;
            Constants.CorePlugin.Kernel = kernel;

            _logger = logger;
            _pluginManagementService = pluginManagementService;
            _rgbService = rgbService;
            _profileService = profileService;
            _moduleService = moduleService;
            _loggingLevel = settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Debug);
            _frameStopWatch = new Stopwatch();
            StartupArguments = new List<string>();
            
            _rgbService.IsRenderPaused = true;
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
            string? argument = StartupArguments.FirstOrDefault(a => a.StartsWith("--logging"));
            if (argument != null)
            {
                // Parse the provided log level
                string[] parts = argument.Split('=');
                if (parts.Length == 2 && Enum.TryParse(typeof(LogEventLevel), parts[1], true, out object? logLevelArgument))
                {
                    _logger.Information("Setting logging level to {loggingLevel} from startup argument", (LogEventLevel) logLevelArgument!);
                    LoggerProvider.LoggingLevelSwitch.MinimumLevel = (LogEventLevel) logLevelArgument;
                }
                else
                {
                    _logger.Warning("Failed to set log level from startup argument {argument}", argument);
                    _logger.Information("Setting logging level to {loggingLevel}", _loggingLevel.Value);
                    LoggerProvider.LoggingLevelSwitch.MinimumLevel = _loggingLevel.Value;
                }
            }
            else
            {
                _logger.Information("Setting logging level to {loggingLevel}", _loggingLevel.Value);
                LoggerProvider.LoggingLevelSwitch.MinimumLevel = _loggingLevel.Value;
            }
        }

        private void SurfaceOnUpdating(UpdatingEventArgs args)
        {
            if (_rgbService.IsRenderPaused)
                return;

            try
            {
                _frameStopWatch.Restart();

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
            }
            catch (Exception e)
            {
                _updateExceptions.Add(e);
            }
            finally
            {
                _rgbService.CloseRender();
                _frameStopWatch.Stop();
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

        public TimeSpan FrameTime { get; private set; }
        public bool ProfileRenderingDisabled { get; set; }
        public List<string> StartupArguments { get; set; }
        public bool IsElevated { get; set; }

        public void Dispose()
        {
            // Dispose services
            _pluginManagementService.Dispose();
        }

        public bool IsInitialized { get; set; }

        public void Initialize()
        {
            if (IsInitialized)
                throw new ArtemisCoreException("Cannot initialize the core as it is already initialized.");

            AssemblyInformationalVersionAttribute? versionAttribute = typeof(CoreService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            _logger.Information(
                "Initializing Artemis Core version {version}, build {buildNumber} branch {branch}.",
                versionAttribute?.InformationalVersion,
                Constants.BuildInfo.BuildNumber,
                Constants.BuildInfo.SourceBranch
            );
            _logger.Information("Startup arguments: {args}", StartupArguments);
            _logger.Information("Elevated permissions: {perms}", IsElevated);

            ApplyLoggingLevel();

            // Don't remove even if it looks useless
            // Just this line should prevent a certain someone from removing HidSharp as an unused dependency as well
            Version? hidSharpVersion = Assembly.GetAssembly(typeof(HidDevice))!.GetName().Version;
            _logger.Debug("Forcing plugins to use HidSharp {hidSharpVersion}", hidSharpVersion);

            DeserializationLogger.Initialize(Kernel!);

            // Initialize the services
            _pluginManagementService.CopyBuiltInPlugins();
            _pluginManagementService.LoadPlugins(StartupArguments, IsElevated);

            _rgbService.IsRenderPaused = false;
            OnInitialized();
        }

        public event EventHandler? Initialized;
        public event EventHandler<FrameRenderingEventArgs>? FrameRendering;
        public event EventHandler<FrameRenderedEventArgs>? FrameRendered;
    }
}