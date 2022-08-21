using Artemis.UI.Shared.Services.Builders;
using Avalonia.Controls;

namespace Artemis.UI.Shared.Services;

internal class NotificationService : INotificationService
{
    private readonly IWindowService _windowService;

    public NotificationService(IWindowService windowService)
    {
        _windowService = windowService;
    }

    public NotificationBuilder CreateNotification()
    {
        Window? currentWindow = _windowService.GetCurrentWindow();
        if (currentWindow == null)
            throw new ArtemisSharedUIException("Can't show an in-app notification without any windows being shown.");

        return new NotificationBuilder(currentWindow);
    }
}