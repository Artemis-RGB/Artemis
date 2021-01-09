using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Ninject;
using Artemis.Core.Services.Core;
using Artemis.Storage;
using HidSharp;
using Newtonsoft.Json;
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
        private readonly ISurfaceService _surfaceService;
        private readonly List<Exception> _updateExceptions = new();
        private List<BaseDataModelExpansion> _dataModelExpansions = new();
        private DateTime _lastExceptionLog;
        private List<Module> _modules = new();

        // ReSharper disable UnusedParameter.Local
        public CoreService(IKernel kernel,
            ILogger logger,
            StorageMigrationService _, // injected to ensure migration runs early
            ISettingsService settingsService,
            IPluginManagementService pluginManagementService,
            IRgbService rgbService,
            ISurfaceService surfaceService,
            IProfileService profileService,
            IModuleService moduleService // injected to ensure module priorities get applied
        )
        {
            Kernel = kernel;
            Constants.CorePlugin.Kernel = kernel;

            _logger = logger;
            _pluginManagementService = pluginManagementService;
            _rgbService = rgbService;
            _surfaceService = surfaceService;
            _profileService = profileService;
            _loggingLevel = settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Debug);
            _renderScale = settingsService.GetSetting("Core.RenderScale", 0.5);
            _frameStopWatch = new Stopwatch();

            UpdatePluginCache();

            _rgbService.Surface.Updating += SurfaceOnUpdating;
            _rgbService.Surface.Updated += SurfaceOnUpdated;
            _loggingLevel.SettingChanged += (sender, args) => ApplyLoggingLevel();

            _pluginManagementService.PluginFeatureEnabled += (sender, args) => UpdatePluginCache();
            _pluginManagementService.PluginFeatureDisabled += (sender, args) => UpdatePluginCache();
        }
        // ReSharper restore UnusedParameter.Local

        public TimeSpan FrameTime { get; private set; }
        public bool ModuleRenderingDisabled { get; set; }
        public List<string>? StartupArguments { get; set; }

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
            _logger.Information("Initializing Artemis Core version {version}, build {buildNumber} branch {branch}.", versionAttribute?.InformationalVersion, Constants.BuildInfo.BuildNumber,
                Constants.BuildInfo.SourceBranch);
            // This should prevent a certain someone from removing HidSharp as an unused dependency as well
            _logger.Information("Forcing plugins to use HidSharp {hidSharpVersion}", Assembly.GetAssembly(typeof(HidDevice))!.GetName().Version);

            ApplyLoggingLevel();

            DeserializationLogger.Initialize(Kernel);

            // Initialize the services
            _pluginManagementService.CopyBuiltInPlugins();
            _pluginManagementService.LoadPlugins(StartupArguments != null && StartupArguments.Contains("--ignore-plugin-lock"));

            ArtemisSurface surfaceConfig = _surfaceService.ActiveSurface;
            _logger.Information("Initialized with active surface entity {surfaceConfig}-{guid}", surfaceConfig.Name, surfaceConfig.EntityId);

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
            IntroAnimation intro = new(_logger, _profileService, _surfaceService);

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
            _logger.Information("Setting logging level to {loggingLevel}", _loggingLevel.Value);
            LoggerProvider.LoggingLevelSwitch.MinimumLevel = _loggingLevel.Value;
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

                // If there is no ready bitmap brush, skip the frame
                if (_rgbService.BitmapBrush == null)
                    return;

                lock (_rgbService.BitmapBrush)
                {
                    if (_rgbService.BitmapBrush.Bitmap == null)
                        return;

                    // Render all active modules
                    using SKCanvas canvas = new(_rgbService.BitmapBrush.Bitmap);
                    canvas.Scale((float) _renderScale.Value);
                    canvas.Clear(new SKColor(0, 0, 0));
                    if (!ModuleRenderingDisabled)
                        // While non-activated modules may be updated above if they expand the main data model, they may never render
                        foreach (Module module in modules.Where(m => m.IsActivated))
                            module.InternalRender(args.DeltaTime, _surfaceService.ActiveSurface, canvas, _rgbService.BitmapBrush.Bitmap.Info);

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

            OnFrameRendered(new FrameRenderedEventArgs(_rgbService.BitmapBrush!, _rgbService.Surface));
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