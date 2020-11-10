using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.JsonConverters;
using Artemis.Core.Ninject;
using Artemis.Storage;
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
        internal static IKernel Kernel;

        private readonly Stopwatch _frameStopWatch;
        private readonly ILogger _logger;
        private readonly PluginSetting<LogEventLevel> _loggingLevel;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IProfileService _profileService;
        private readonly IRgbService _rgbService;
        private readonly ISurfaceService _surfaceService;
        private List<BaseDataModelExpansion> _dataModelExpansions;
        private List<Module> _modules;

        // ReSharper disable once UnusedParameter.Local - Storage migration service is injected early to ensure it runs before anything else
        public CoreService(IKernel kernel, ILogger logger, StorageMigrationService _, ISettingsService settingsService, IPluginManagementService pluginManagementService,
            IRgbService rgbService, ISurfaceService surfaceService, IProfileService profileService, IModuleService moduleService)
        {
            Kernel = kernel;
            Constants.CorePluginInfo.Kernel = kernel;

            _logger = logger;
            _pluginManagementService = pluginManagementService;
            _rgbService = rgbService;
            _surfaceService = surfaceService;
            _profileService = profileService;
            _loggingLevel = settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Debug);
            _frameStopWatch = new Stopwatch();

            UpdatePluginCache();
            ConfigureJsonConvert();

            _rgbService.Surface.Updating += SurfaceOnUpdating;
            _rgbService.Surface.Updated += SurfaceOnUpdated;
            _loggingLevel.SettingChanged += (sender, args) => ApplyLoggingLevel();

            _pluginManagementService.PluginEnabled += (sender, args) => UpdatePluginCache();
            _pluginManagementService.PluginDisabled += (sender, args) => UpdatePluginCache();
        }

        public TimeSpan FrameTime { get; private set; }
        public bool ModuleRenderingDisabled { get; set; }
        public List<string> StartupArguments { get; set; }

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
            _logger.Information("Initializing Artemis Core version {version}", versionAttribute?.InformationalVersion);
            ApplyLoggingLevel();

            DeserializationLogger.Initialize(Kernel);

            // Initialize the services
            _pluginManagementService.CopyBuiltInPlugins();
            _pluginManagementService.LoadPlugins(StartupArguments.Contains("--ignore-plugin-lock"));

            ArtemisSurface surfaceConfig = _surfaceService.ActiveSurface;
            if (surfaceConfig != null)
                _logger.Information("Initialized with active surface entity {surfaceConfig}-{guid}", surfaceConfig.Name, surfaceConfig.EntityId);
            else
                _logger.Information("Initialized without an active surface entity");

            PlayIntroAnimation();
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

        private void PlayIntroAnimation()
        {
            // The intro is cool and all, but sometimes you just wanna see what you're working on straight away ^^
            if (Debugger.IsAttached)
                return;

            IntroAnimation intro = new IntroAnimation(_logger, _profileService, _surfaceService);

            // Draw a white overlay over the device
            void DrawOverlay(object? sender, FrameRenderingEventArgs args) => intro.Render(args.DeltaTime, args.Canvas);
            FrameRendering += DrawOverlay;

            // Stop rendering after the profile finishes (take 1 second extra in case of slow updates)
            TimeSpan introLength = intro.AnimationProfile.GetAllLayers().Max(l => l.Timeline.Length);
            Task.Run(async () =>
            {
                await Task.Delay(introLength.Add(TimeSpan.FromSeconds(1)));
                FrameRendering -= DrawOverlay;

                intro.AnimationProfile.Dispose();
            });
        }

        private void UpdatePluginCache()
        {
            _modules = _pluginManagementService.GetPluginsOfType<Module>().Where(p => p.IsEnabled).ToList();
            _dataModelExpansions = _pluginManagementService.GetPluginsOfType<BaseDataModelExpansion>().Where(p => p.IsEnabled).ToList();
        }

        private void ConfigureJsonConvert()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new SKColorConverter(), new ForgivingIntConverter()}
            };
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
                        dataModelExpansion.Update(args.DeltaTime);
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
                    using SKCanvas canvas = new SKCanvas(_rgbService.BitmapBrush.Bitmap);
                    canvas.Clear(new SKColor(0, 0, 0));
                    if (!ModuleRenderingDisabled)
                    {
                        // While non-activated modules may be updated above if they expand the main data model, they may never render
                        foreach (Module module in modules.Where(m => m.IsActivated))
                            module.InternalRender(args.DeltaTime, _surfaceService.ActiveSurface, canvas, _rgbService.BitmapBrush.Bitmap.Info);
                    }

                    OnFrameRendering(new FrameRenderingEventArgs(modules, canvas, args.DeltaTime, _rgbService.Surface));
                }
            }
            catch (Exception e)
            {
                throw new ArtemisCoreException("Exception during update", e);
            }
            finally
            {
                _frameStopWatch.Stop();
                FrameTime = _frameStopWatch.Elapsed;
            }
        }

        private void SurfaceOnUpdated(UpdatedEventArgs args)
        {
            if (_rgbService.IsRenderPaused)
                return;

            OnFrameRendered(new FrameRenderedEventArgs(_rgbService.BitmapBrush, _rgbService.Surface));
        }

        #region Events

        public event EventHandler Initialized;
        public event EventHandler<FrameRenderingEventArgs> FrameRendering;
        public event EventHandler<FrameRenderedEventArgs> FrameRendered;

        private void OnInitialized()
        {
            IsInitialized = true;
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}