using Artemis.UI.Avalonia.Shared.Services.Builders;
using Artemis.UI.Avalonia.Shared.Services.Interfaces;

namespace Artemis.UI.Avalonia.Shared.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IWindowService _windowService;

        public NotificationService(IWindowService windowService)
        {
            _windowService = windowService;
        }

        public NotificationBuilder CreateNotification()
        {
            return new NotificationBuilder(_windowService.GetCurrentWindow());
        }
    }
}