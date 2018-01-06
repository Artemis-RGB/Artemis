using System.Threading.Tasks;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services
{
    public class CoreService : ICoreService
    {
        private readonly IModuleService _moduleService;

        public CoreService(IModuleService moduleService)
        {
            _moduleService = moduleService;
            Task.Run(Initialize);
        }

        public void Dispose()
        {
            _moduleService.Dispose();
        }

        public bool IsInitialized { get; set; }

        private async Task Initialize()
        {
            await _moduleService.LoadModules();

            IsInitialized = true;
        }
    }
}