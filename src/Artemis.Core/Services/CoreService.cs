using System.Threading.Tasks;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services
{
    public class CoreService : ICoreService
    {
        private readonly IPluginService _pluginService;

        public CoreService(IPluginService pluginService)
        {
            _pluginService = pluginService;
            Task.Run(Initialize);
        }

        public void Dispose()
        {
            _pluginService.Dispose();
        }

        public bool IsInitialized { get; set; }

        private async Task Initialize()
        {
            await _pluginService.LoadModules();

            IsInitialized = true;
        }
    }
}