using Artemis.UI.Shared.Services.Builders;

namespace Artemis.UI.Shared.Services.Interfaces
{
    public interface INotificationService : IArtemisSharedUIService
    {
        NotificationBuilder CreateNotification();
    }
}