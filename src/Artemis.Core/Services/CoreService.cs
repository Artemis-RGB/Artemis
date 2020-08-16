using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.JsonConverters;
using Artemis.Core.Models.Profile;
using Artemis.Core.Ninject;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Core.Utilities;
using Artemis.Storage;
using Newtonsoft.Json;
using RGB.NET.Core;
using Serilog;
using Serilog.Events;
using SkiaSharp;
using Module = Artemis.Core.Plugins.Abstract.Module;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides Artemis's core update loop
    /// </summary>
    public class CoreService : ICoreService
    {
        private readonly Stopwatch _frameStopWatch;
        private readonly ILogger _logger;
        private readonly PluginSetting<LogEventLevel> _loggingLevel;
        private readonly IPluginService _pluginService;
        private readonly IProfileService _profileService;
        private readonly IRgbService _rgbService;
        private readonly ISurfaceService _surfaceService;
        private List<BaseDataModelExpansion> _dataModelExpansions;
        private List<Module> _modules;
        private IntroAnimation _introAnimation;
        private DateTime _lastModuleActivationUpdate;

        // ReSharper disable once UnusedParameter.Local - Storage migration service is injected early to ensure it runs before anything else
        internal CoreService(ILogger logger, StorageMigrationService _, ISettingsService settingsService, IPluginService pluginService,
            IRgbService rgbService, ISurfaceService surfaceService, IProfileService profileService)
        {
            _logger = logger;
            _pluginService = pluginService;
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

            _pluginService.PluginEnabled += (sender, args) => UpdatePluginCache();
            _pluginService.PluginDisabled += (sender, args) => UpdatePluginCache();
        }

        public TimeSpan FrameTime { get; private set; }
        public bool ModuleRenderingDisabled { get; set; }
        public List<string> StartupArguments { get; set; }

        public void Dispose()
        {
            // Dispose services
            _pluginService.Dispose();
        }

        public bool IsInitialized { get; set; }

        public void Initialize()
        {
            if (IsInitialized)
                throw new ArtemisCoreException("Cannot initialize the core as it is already initialized.");

            var versionAttribute = typeof(CoreService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            _logger.Information("Initializing Artemis Core version {version}", versionAttribute?.InformationalVersion);
            ApplyLoggingLevel();

            // Initialize the services
            _pluginService.CopyBuiltInPlugins();
            _pluginService.LoadPlugins(StartupArguments.Contains("--ignore-plugin-lock"));

            var surfaceConfig = _surfaceService.ActiveSurface;
            if (surfaceConfig != null)
                _logger.Information("Initialized with active surface entity {surfaceConfig}-{guid}", surfaceConfig.Name, surfaceConfig.EntityId);
            else
                _logger.Information("Initialized without an active surface entity");

            PlayIntroAnimation();
            OnInitialized();
        }

        private void PlayIntroAnimation()
        {
            FrameRendering += DrawOverlay;
            _introAnimation = new IntroAnimation(_logger, _profileService, _surfaceService);

            // Draw a white overlay over the device
            void DrawOverlay(object sender, FrameRenderingEventArgs args)
            {
                if (_introAnimation == null)
                    args.Canvas.Clear(new SKColor(0, 0, 0));
                else
                    _introAnimation.Render(args.DeltaTime, args.Canvas, _rgbService.BitmapBrush.Bitmap.Info);
            }

            var introLength = _introAnimation.AnimationProfile.GetAllLayers().Max(l => l.TimelineLength);

            // Stop rendering after the profile finishes (take 1 second extra in case of slow updates)
            Task.Run(async () =>
            {
                await Task.Delay(introLength.Add(TimeSpan.FromSeconds(1)));
                FrameRendering -= DrawOverlay;

                _introAnimation.AnimationProfile?.Dispose();
                _introAnimation = null;
            });
        }

        public void SetMainWindowHandle(IntPtr handle)
        {
            Constants.MainWindowHandle = handle;
        }

        protected virtual void OnFrameRendering(FrameRenderingEventArgs e)
        {
            FrameRendering?.Invoke(this, e);
        }

        protected virtual void OnFrameRendered(FrameRenderedEventArgs e)
        {
            FrameRendered?.Invoke(this, e);
        }

        private void UpdatePluginCache()
        {
            _modules = _pluginService.GetPluginsOfType<Module>().Where(p => p.Enabled).ToList();
            _dataModelExpansions = _pluginService.GetPluginsOfType<BaseDataModelExpansion>().Where(p => p.Enabled).ToList();
        }

        private void ConfigureJsonConvert()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new SKColorConverter()}
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

                // Only run the module activation update every 2 seconds
                if (DateTime.Now - _lastModuleActivationUpdate > TimeSpan.FromSeconds(2))
                    ModuleActivationUpdate();

                lock (_dataModelExpansions)
                {
                    // Update all active modules
                    foreach (var dataModelExpansion in _dataModelExpansions)
                        dataModelExpansion.Update(args.DeltaTime);
                }

                List<Module> modules;
                lock (_modules)
                {
                    modules = _modules.Where(m => m.IsActivated).ToList();
                }

                // Update all active modules
                foreach (var module in modules)
                    module.Update(args.DeltaTime);

                // If there is no ready bitmap brush, skip the frame
                if (_rgbService.BitmapBrush == null)
                    return;

                lock (_rgbService.BitmapBrush)
                {
                    if (_rgbService.BitmapBrush.Bitmap == null)
                        return;

                    // Render all active modules
                    using var canvas = new SKCanvas(_rgbService.BitmapBrush.Bitmap);
                    canvas.Clear(new SKColor(0, 0, 0));
                    if (!ModuleRenderingDisabled)
                    {
                        foreach (var module in modules)
                            module.Render(args.DeltaTime, _surfaceService.ActiveSurface, canvas, _rgbService.BitmapBrush.Bitmap.Info);
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

        private void ModuleActivationUpdate()
        {
            _lastModuleActivationUpdate = DateTime.Now;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            lock (_modules)
            {
                foreach (var module in _modules)
                {
                    var shouldBeActivated = module.EvaluateActivationRequirements();
                    if (shouldBeActivated && !module.IsActivated)
                    {
                        module.Activate();
                        // If this is a profile module, activate the last active profile after module activation
                        if (module is ProfileModule profileModule)
                            _profileService.ActivateLastProfile(profileModule);
                    }
                    else if (!shouldBeActivated && module.IsActivated)
                        module.Deactivate();
                }
            }

            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 100)
                _logger.Warning("Activation requirements evaluation took too long: {moduleCount} module(s) in {elapsed}", _modules.Count, stopwatch.Elapsed);
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