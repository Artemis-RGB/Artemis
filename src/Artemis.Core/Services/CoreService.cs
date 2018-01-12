using System;
using System.Threading.Tasks;
using Artemis.Core.Exceptions;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services
{
    public class CoreService : ICoreService
    {
        private readonly IRgbService _rgbService;
        private readonly IPluginService _pluginService;

        public CoreService(IPluginService pluginService, IRgbService rgbService)
        {
            _pluginService = pluginService;
            _rgbService = rgbService;

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
            await _pluginService.LoadPlugins();
            await _rgbService.LoadDevices();

            OnInitialized();
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