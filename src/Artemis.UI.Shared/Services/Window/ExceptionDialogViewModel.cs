using System;
using System.Threading.Tasks;
using Artemis.UI.Shared.Services.Builders;
using Avalonia.Layout;

namespace Artemis.UI.Shared.Services;

internal class ExceptionDialogViewModel : DialogViewModelBase<object>
{
    private readonly INotificationService _notificationService;

    public ExceptionDialogViewModel(string title, Exception exception, string? customMessage, INotificationService notificationService)
    {
        _notificationService = notificationService;

        Title = $"Artemis | {title}";
        Message = customMessage ?? "It looks like Artemis ran into an unexpected error. If this keeps happening feel free to hit us up on Discord.";
        Exception = exception;
    }

    public string Title { get; }
    public string Message { get; }
    public Exception Exception { get; }

    public async Task CopyException()
    {
        await UI.Clipboard.SetTextAsync(Exception.ToString());
        _notificationService.CreateNotification()
            .WithMessage("Copied stack trace to clipboard.")
            .WithSeverity(NotificationSeverity.Success)
            .WithHorizontalPosition(HorizontalAlignment.Left)
            .Show();
    }
}