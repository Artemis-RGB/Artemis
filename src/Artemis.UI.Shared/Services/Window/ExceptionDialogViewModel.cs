﻿using System;
using System.Threading.Tasks;
using Artemis.UI.Shared.Services.Builders;
using Avalonia;
using Avalonia.Layout;

namespace Artemis.UI.Shared.Services;

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
        await UI.Clipboard.SetTextAsync(Exception.ToString());
        _notificationService.CreateNotification()
            .WithMessage("Copied stack trace to clipboard.")
            .WithSeverity(NotificationSeverity.Success)
            .WithHorizontalPosition(HorizontalAlignment.Left)
            .Show();
    }
}