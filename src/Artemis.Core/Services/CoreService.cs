using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.JsonConverters;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Newtonsoft.Json;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides Artemis's core update loop
    /// </summary>
    public class CoreService : ICoreService
    {
        private readonly ILogger _logger;
        private readonly IPluginService _pluginService;
        private readonly IProfileService _profileService;
        private readonly IRgbService _rgbService;
        private readonly ISurfaceService _surfaceService;
        private List<Module> _modules;

        internal CoreService(ILogger logger, IPluginService pluginService, IRgbService rgbService, ISurfaceService surfaceService, IProfileService profileService)
        {
            _logger = logger;
            _pluginService = pluginService;
            _rgbService = rgbService;
            _surfaceService = surfaceService;
            _profileService = profileService;
            _rgbService.Surface.Updating += SurfaceOnUpdating;
            _rgbService.Surface.Updated += SurfaceOnUpdated;

            _modules = _pluginService.GetPluginsOfType<Module>();
            _pluginService.PluginEnabled += (sender, args) => _modules = _pluginService.GetPluginsOfType<Module>();
            _pluginService.PluginDisabled += (sender, args) => _modules = _pluginService.GetPluginsOfType<Module>();

            ConfigureJsonConvert();
            Task.Run(Initialize);
        }

        public bool ModuleUpdatingDisabled { get; set; }
        public bool ModuleRenderingDisabled { get; set; }

        public void Dispose()
        {
            // Dispose services
            _pluginService.Dispose();
        }

        public bool IsInitialized { get; set; }

        private void ConfigureJsonConvert()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new SKColorConverter()}
            };
        }

        private async Task Initialize()
        {
            if (IsInitialized)
                throw new ArtemisCoreException("Cannot initialize the core as it is already initialized.");

            _logger.Information("Initializing Artemis Core version {version}", typeof(CoreService).Assembly.GetName().Version);

            // Initialize the services
            await Task.Run(() => _pluginService.CopyBuiltInPlugins());
            await Task.Run(() => _pluginService.LoadPlugins());

            var surfaceConfig = _surfaceService.ActiveSurface;
            if (surfaceConfig != null)
                _logger.Information("Initialized with active surface entity {surfaceConfig}-{guid}", surfaceConfig.Name, surfaceConfig.EntityId);
            else
                _logger.Information("Initialized without an active surface entity");

            await Task.Run(() => _profileService.ActivateDefaultProfiles());

            OnInitialized();
        }

        private void SurfaceOnUpdating(UpdatingEventArgs args)
        {
            try
            {
                if (!ModuleUpdatingDisabled)
                {
                    lock (_modules)
                    {
                        // Update all active modules
                        foreach (var module in _modules)
                            module.Update(args.DeltaTime);
                    }
                }

                // If there is no ready bitmap brush, skip the frame
                if (_rgbService.BitmapBrush == null)
                    return;

                lock (_rgbService.BitmapBrush)
                {
                    if (_rgbService.BitmapBrush.Bitmap == null)
                        return;

                    // Render all active modules
                    using (var canvas = new SKCanvas(_rgbService.BitmapBrush.Bitmap))
                    {
                        canvas.Clear(new SKColor(0, 0, 0));
                        if (!ModuleRenderingDisabled)
                        {
                            lock (_modules)
                            {
                                foreach (var module in _modules)
                                    module.Render(args.DeltaTime, _surfaceService.ActiveSurface, canvas);
                            }
                        }

                        OnFrameRendering(new FrameRenderingEventArgs(_modules, canvas, args.DeltaTime, _rgbService.Surface));
                    }
                }
            }
            catch (Exception e)
            {
                throw new ArtemisCoreException("Exception during update", e);
            }
        }

        private void SurfaceOnUpdated(UpdatedEventArgs args)
        {
            OnFrameRendered(new FrameRenderedEventArgs(_rgbService.BitmapBrush, _rgbService.Surface));
        }

        protected virtual void OnFrameRendering(FrameRenderingEventArgs e)
        {
            FrameRendering?.Invoke(this, e);
        }

        protected virtual void OnFrameRendered(FrameRenderedEventArgs e)
        {
            FrameRendered?.Invoke(this, e);
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