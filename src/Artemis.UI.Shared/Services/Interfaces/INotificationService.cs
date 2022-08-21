using Artemis.UI.Shared.Services.Builders;

namespace Artemis.UI.Shared.Services;

/// <summary>
///     A service that can be used to create notifications in either the application or on the desktop.
/// </summary>
public interface INotificationService : IArtemisSharedUIService
{
    /// <summary>
    ///     Creates an in-app notification using a builder.
    /// </summary>
    /// <returns>A builder used to configure and show the notification.</returns>
    NotificationBuilder CreateNotification();
}