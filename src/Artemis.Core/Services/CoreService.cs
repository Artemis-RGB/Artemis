using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core.DataModelExpansions;
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
        internal static IKernel Kernel = null!;

        private readonly Stopwatch _frameStopWatch;
        private readonly ILogger _logger;
        private readonly PluginSetting<LogEventLevel> _loggingLevel;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IProfileService _profileService;
        private readonly PluginSetting<double> _renderScale;
        private readonly IRgbService _rgbService;
        private readonly List<Exception> _updateExceptions = new();
        private List<BaseDataModelExpansion> _dataModelExpansions = new();
        private DateTime _lastExceptionLog;
        private List<Module> _modules = new();
        private SKBitmap? _bitmap;
        private readonly object _bitmapLock = new();

        // ReSharper disable UnusedParameter.Local
        public CoreService(IKernel kernel,
            ILogger logger,
            StorageMigrationService _, // injected to ensure migration runs early
            ISettingsService settingsService,
            IPluginManagementService pluginManagementService,
            IRgbService rgbService,
            IProfileService profileService,
            IModuleService moduleService // injected to ensure module priorities get applied
        )
        {
            Kernel = kernel;
            Constants.CorePlugin.Kernel = kernel;

            _logger = logger;
            _pluginManagementService = pluginManagementService;
            _rgbService = rgbService;
            _profileService = profileService;
            _loggingLevel = settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Debug);
            _renderScale = settingsService.GetSetting("Core.RenderScale", 0.5);
            _frameStopWatch = new Stopwatch();
            StartupArguments = new List<string>();

            UpdatePluginCache();

            _rgbService.Surface.Updating += SurfaceOnUpdating;
            _rgbService.Surface.Updated += SurfaceOnUpdated;
            _rgbService.Surface.SurfaceLayoutChanged += SurfaceOnSurfaceLayoutChanged;
            _loggingLevel.SettingChanged += (sender, args) => ApplyLoggingLevel();
            _renderScale.SettingChanged += RenderScaleSettingChanged;

            _pluginManagementService.PluginFeatureEnabled += (sender, args) => UpdatePluginCache();
            _pluginManagementService.PluginFeatureDisabled += (sender, args) => UpdatePluginCache();
        }

        // ReSharper restore UnusedParameter.Local

        public TimeSpan FrameTime { get; private set; }
        public bool ModuleRenderingDisabled { get; set; }
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

            DeserializationLogger.Initialize(Kernel);

            // Initialize the services
            _pluginManagementService.CopyBuiltInPlugins();
            _pluginManagementService.LoadPlugins(
                StartupArguments.Contains("--ignore-plugin-lock"),
                StartupArguments.Contains("--force-elevation"),
                IsElevated
            );

            OnInitialized();
        }

        protected virtual void OnFrameRendering(FrameRenderingEventArgs e)
        {
            FrameRendering?.Invoke(this, e);
        }

        protected virtual void OnFrameRendered(FrameRenderedEventArgs e)
        {
            FrameRendered?.Invoke(this, e);
        }

        public void PlayIntroAnimation()
        {
            IntroAnimation intro = new(_logger, _profileService, _rgbService.EnabledDevices);

            // Draw a white overlay over the device
            void DrawOverlay(object? sender, FrameRenderingEventArgs args)
            {
                intro.Render(args.DeltaTime, args.Canvas);
            }

            FrameRendering += DrawOverlay;

            // Stop rendering after the profile finishes (take 1 second extra in case of slow updates)
            TimeSpan introLength = intro.AnimationProfile.GetAllLayers().Max(l => l.Timeline.Length)!;
            Task.Run(async () =>
            {
                await Task.Delay(introLength.Add(TimeSpan.FromSeconds(1)));
                FrameRendering -= DrawOverlay;

                intro.AnimationProfile.Dispose();
            });
        }

        private void UpdatePluginCache()
        {
            _modules = _pluginManagementService.GetFeaturesOfType<Module>().Where(p => p.IsEnabled).ToList();
            _dataModelExpansions = _pluginManagementService.GetFeaturesOfType<BaseDataModelExpansion>().Where(p => p.IsEnabled).ToList();
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
                    _logger.Information("Setting logging level to {loggingLevel} from startup argument", (LogEventLevel)logLevelArgument!);
                    LoggerProvider.LoggingLevelSwitch.MinimumLevel = (LogEventLevel)logLevelArgument;
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
                lock (_dataModelExpansions)
                {
                    // Update all active modules, check Enabled status because it may go false before before the _dataModelExpansions list is updated
                    foreach (BaseDataModelExpansion dataModelExpansion in _dataModelExpansions.Where(e => e.IsEnabled))
                        dataModelExpansion.InternalUpdate(args.DeltaTime);
                }

                List<Module> modules;
                lock (_modules)
                {
                    modules = _modules.Where(m => m.IsActivated || m.InternalExpandsMainDataModel)
                        .OrderBy(m => m.PriorityCategory)
                        .ThenByDescending(m => m.Priority)
                        .ToList();
                }

                // Update all active modules
                foreach (Module module in modules)
                    module.InternalUpdate(args.DeltaTime);

                lock (_bitmapLock)
                {
                    if (_bitmap == null)
                    {
                        _bitmap = CreateBitmap();
                        _rgbService.UpdateTexture(_bitmap);
                    }

                    // Render all active modules
                    using SKCanvas canvas = new(_bitmap);
                    canvas.Scale((float)_renderScale.Value);
                    canvas.Clear(new SKColor(0, 0, 0));
                    if (!ModuleRenderingDisabled)
                        // While non-activated modules may be updated above if they expand the main data model, they may never render
                        foreach (Module module in modules.Where(m => m.IsActivated))
                            module.InternalRender(args.DeltaTime, canvas, _bitmap.Info);

                    OnFrameRendering(new FrameRenderingEventArgs(canvas, args.DeltaTime, _rgbService.Surface));
                }
            }
            catch (Exception e)
            {
                _updateExceptions.Add(e);
            }
            finally
            {
                _frameStopWatch.Stop();
                FrameTime = _frameStopWatch.Elapsed;

                LogUpdateExceptions();
            }
        }

        private SKBitmap CreateBitmap()
        {
            float width = MathF.Min(_rgbService.Surface.Boundary.Size.Width * (float)_renderScale.Value, 4096);
            float height = MathF.Min(_rgbService.Surface.Boundary.Size.Height * (float)_renderScale.Value, 4096);
            return new SKBitmap(new SKImageInfo(width.RoundToInt(), height.RoundToInt(), SKColorType.Rgb888x));
        }

        private void InvalidateBitmap()
        {
            lock (_bitmapLock)
            {
                _bitmap = null;
            }
        }

        private void SurfaceOnSurfaceLayoutChanged(SurfaceLayoutChangedEventArgs args) => InvalidateBitmap();
        private void RenderScaleSettingChanged(object? sender, EventArgs e) => InvalidateBitmap();

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

        private void SurfaceOnUpdated(UpdatedEventArgs args)
        {
            if (_rgbService.IsRenderPaused)
                return;

            OnFrameRendered(new FrameRenderedEventArgs(_rgbService.Texture!, _rgbService.Surface));
        }

        #region Events

        public event EventHandler? Initialized;
        public event EventHandler<FrameRenderingEventArgs>? FrameRendering;
        public event EventHandler<FrameRenderedEventArgs>? FrameRendered;

        private void OnInitialized()
        {
            IsInitialized = true;
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}