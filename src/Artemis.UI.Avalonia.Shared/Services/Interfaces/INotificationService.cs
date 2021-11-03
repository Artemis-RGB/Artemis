using Artemis.UI.Avalonia.Shared.Services.Builders;

namespace Artemis.UI.Avalonia.Shared.Services.Interfaces
{
    public interface INotificationService : IArtemisSharedUIService
    {
        NotificationBuilder CreateNotification();
    }
}