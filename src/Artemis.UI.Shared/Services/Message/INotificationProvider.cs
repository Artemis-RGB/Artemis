using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Shared.Services
{
    /// <summary>
    ///     Represents a class provides desktop notifications so that <see cref="IMessageService" /> can us it to show desktop
    ///     notifications
    /// </summary>
    public interface INotificationProvider
    {
        /// <summary>
        ///     Shows a notification
        /// </summary>
        /// <param name="title">The title of the notification</param>
        /// <param name="message">The message of the notification</param>
        /// <param name="icon">The Material Design icon to show in the notification</param>
        void ShowNotification(string title, string message, PackIconKind icon);
    }
}