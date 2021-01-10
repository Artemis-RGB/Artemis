using System;
using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Shared.Services
{
    internal class MessageService : IMessageService
    {
        private INotificationProvider _notificationProvider;
        public ISnackbarMessageQueue MainMessageQueue { get; }

        public MessageService(ISnackbarMessageQueue mainMessageQueue)
        {
            MainMessageQueue = mainMessageQueue;
        }

        /// <inheritdoc />
        public void ConfigureNotificationProvider(INotificationProvider notificationProvider)
        {
            _notificationProvider = notificationProvider;
        }

        public void ShowMessage(object content)
        {
            MainMessageQueue.Enqueue(content);
        }

        public void ShowMessage(object content, object actionContent, Action actionHandler)
        {
            MainMessageQueue.Enqueue(content, actionContent, actionHandler);
        }

        public void ShowMessage<TArgument>(object content, object actionContent, Action<TArgument> actionHandler, TArgument actionArgument)
        {
            MainMessageQueue.Enqueue(content, actionContent, actionHandler, actionArgument);
        }

        public void ShowMessage(object content, bool neverConsiderToBeDuplicate)
        {
            MainMessageQueue.Enqueue(content, neverConsiderToBeDuplicate);
        }

        public void ShowMessage(object content, object actionContent, Action actionHandler, bool promote)
        {
            MainMessageQueue.Enqueue(content, actionContent, actionHandler, promote);
        }

        public void ShowMessage<TArgument>(object content, object actionContent, Action<TArgument> actionHandler, TArgument actionArgument, bool promote)
        {
            MainMessageQueue.Enqueue(content, actionContent, actionHandler, actionArgument, promote);
        }

        public void ShowMessage<TArgument>(object content,
            object actionContent,
            Action<TArgument> actionHandler,
            TArgument actionArgument,
            bool promote,
            bool neverConsiderToBeDuplicate,
            TimeSpan? durationOverride = null)
        {
            MainMessageQueue.Enqueue(content, actionContent, actionHandler, actionArgument, promote, neverConsiderToBeDuplicate, durationOverride);
        }

        public void ShowMessage(object content,
            object actionContent,
            Action<object> actionHandler,
            object actionArgument,
            bool promote,
            bool neverConsiderToBeDuplicate,
            TimeSpan? durationOverride = null)
        {
            MainMessageQueue.Enqueue(content, actionContent, actionHandler, actionArgument, promote, neverConsiderToBeDuplicate, durationOverride);
        }

        /// <inheritdoc />
        public void ShowNotification(string title, string message)
        {
            _notificationProvider.ShowNotification(title, message, PackIconKind.None);
        }

        /// <inheritdoc />
        public void ShowNotification(string title, string message, PackIconKind icon)
        {
            _notificationProvider.ShowNotification(title, message, icon);
        }

        /// <inheritdoc />
        public void ShowNotification(string title, string message, string icon)
        {
            Enum.TryParse(typeof(PackIconKind), icon, true, out object? iconKind);
            _notificationProvider.ShowNotification(title, message, (PackIconKind) (iconKind ?? PackIconKind.None));
        }
    }
}