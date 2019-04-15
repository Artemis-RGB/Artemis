using System;
using System.Threading.Tasks;
using Artemis.Core.Exceptions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using Color = System.Drawing.Color;

namespace Artemis.Core.Services
{
    public class CoreService : ICoreService
    {
        private readonly IPluginService _pluginService;
        private readonly IRgbService _rgbService;

        public CoreService(IPluginService pluginService, IRgbService rgbService)
        {
            _pluginService = pluginService;
            _rgbService = rgbService;
            _rgbService.Surface.Updating += SurfaceOnUpdating;


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

            // Initialize the services
            await Task.Run(() => _pluginService.LoadPlugins());
            await _rgbService.LoadDevices();

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

                if (_rgbService.GraphicsDecorator == null)
                    return;

                // Render all active modules
                using (var g = _rgbService.GraphicsDecorator.GetGraphics())
                {
                    g.Clear(Color.Red);

                    foreach (var module in modules)
                        module.Render(args.DeltaTime, _rgbService.Surface, g);
                }
            }
            catch (Exception e)
            {
                throw new ArtemisCoreException("Exception during update", e);
            }
        }

        #region Events

        public event EventHandler Initialized;

        private void OnInitialized()
        {
            IsInitialized = true;
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}