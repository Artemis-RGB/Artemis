using Artemis.Core.Services;

namespace Artemis.UI.Services
{
    public class RemoteManagementService : IRemoteManagementService
    {
        public RemoteManagementService(IWebServerService webServerService)
        {
            webServerService.AddController<RemoteController>();
        }
    }
}