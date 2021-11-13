using System;
using System.Threading.Tasks;
using Artemis.UI.Avalonia.Shared.Services.Builders;
using Artemis.UI.Avalonia.Shared.Services.Interfaces;
using Avalonia;
using Avalonia.Layout;

namespace Artemis.UI.Avalonia.Shared.Services
{
    internal class ExceptionDialogViewModel : DialogViewModelBase<object>
    {
        private readonly INotificationService _notificationService;

        public ExceptionDialogViewModel(string title, Exception exception, INotificationService notificationService)
        {
            _notificationService = notificationService;

            Title = $"Artemis | {title}";
            Exception = exception;
        }

        public string Title { get; }
        public Exception Exception { get; }

        public async Task CopyException()
        {
            await Application.Current.Clipboard.SetTextAsync(Exception.ToString());
            _notificationService.CreateNotification()
                .WithMessage("Copied stack trace to clipboard.")
                .WithSeverity(NotificationSeverity.Success)
                .WithHorizontalPosition(HorizontalAlignment.Center)
                .Show();
        }
    }
}