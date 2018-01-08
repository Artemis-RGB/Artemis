using System.Threading.Tasks;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services
{
    public class CoreService : ICoreService
    {
        private readonly IPluginService _pluginService;
        private readonly IDeviceService _deviceService;

        public CoreService(IPluginService pluginService, IDeviceService deviceService)
        {
            _pluginService = pluginService;
            _deviceService = deviceService;
            Task.Run(Initialize);
        }

        public void Dispose()
        {
            _pluginService.Dispose();
        }

        public bool IsInitialized { get; set; }

        private async Task Initialize()
        {
            await _pluginService.LoadPlugins();
            await _deviceService.LoadDevices();
            IsInitialized = true;
        }
    }
}