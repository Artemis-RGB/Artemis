using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage;
using RGB.NET.Core;
using Serilog;
using Color = System.Drawing.Color;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides Artemis's core update loop
    /// </summary>
    public class CoreService : ICoreService
    {
        private readonly ILogger _logger;
        private readonly IPluginService _pluginService;
        private readonly IRgbService _rgbService;
        private readonly ISurfaceService _surfaceService;

        internal CoreService(ILogger logger, IPluginService pluginService, IRgbService rgbService, ISurfaceService surfaceService)
        {
            _logger = logger;
            _pluginService = pluginService;
            _rgbService = rgbService;
            _surfaceService = surfaceService;
            _rgbService.Surface.Updating += SurfaceOnUpdating;
            _rgbService.Surface.Updated += SurfaceOnUpdated;

            Task.Run(Initialize);
        }

        public void Dispose()
        {
            // Dispose services
            _pluginService.Dispose();
        }

        public bool IsInitialized { get; set; }

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
                _logger.Information("Initialized with active surface entity {surfaceConfig}-{guid}", surfaceConfig.Name, surfaceConfig.Guid);
            else
                _logger.Information("Initialized without an active surface entity");

            OnInitialized();
        }

        private void SurfaceOnUpdating(UpdatingEventArgs args)
        {
            try
            {
                var modules = _pluginService.GetPluginsOfType<Module>();

                // Update all active modules
                foreach (var module in modules)
                    module.Update(args.DeltaTime);

                // If there is no graphics decorator, skip the frame
                if (_rgbService.GraphicsDecorator == null)
                    return;

                // Render all active modules
                using (var g = _rgbService.GraphicsDecorator.GetGraphics())
                {
                    // If there are no graphics, skip the frame
                    if (g == null)
                        return;

                    g.Clear(Color.Black);
                    foreach (var module in modules)
                        module.Render(args.DeltaTime, _surfaceService.ActiveSurface, g);
                }

                OnFrameRendering(new FrameRenderingEventArgs(modules, _rgbService.GraphicsDecorator.GetBitmap(), args.DeltaTime, _rgbService.Surface));
            }
            catch (Exception e)
            {
                throw new ArtemisCoreException("Exception during update", e);
            }
        }

        private void SurfaceOnUpdated(UpdatedEventArgs args)
        {
            OnFrameRendered(new FrameRenderedEventArgs(_rgbService.GraphicsDecorator.GetBitmap(), _rgbService.Surface));
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

        protected virtual void OnFrameRendering(FrameRenderingEventArgs e)
        {
            FrameRendering?.Invoke(this, e);
        }

        protected virtual void OnFrameRendered(FrameRenderedEventArgs e)
        {
            FrameRendered?.Invoke(this, e);
        }
    }
}