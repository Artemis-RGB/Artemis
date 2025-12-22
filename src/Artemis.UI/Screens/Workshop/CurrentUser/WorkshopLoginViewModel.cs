using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.CurrentUser;

public class WorkshopLoginViewModel : ContentDialogViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly CancellationTokenSource _cts;
    private readonly INotificationService _notificationService;

    public WorkshopLoginViewModel(IAuthenticationService authenticationService, INotificationService notificationService)
    {
        _authenticationService = authenticationService;
        _notificationService = notificationService;
        _cts = new CancellationTokenSource();

        this.WhenActivated(d =>
        {
            Dispatcher.UIThread.InvokeAsync(Login);
            Disposable.Create(_cts, cts => cts.Cancel()).DisposeWith(d);
        });
    }

    private async Task Login()
    {
        try
        {
            await _authenticationService.Login(_cts.Token);
            _notificationService.CreateNotification().WithTitle("Login succeeded").WithSeverity(NotificationSeverity.Success).Show();
            ContentDialog?.Hide(ContentDialogResult.Primary);
        }
        catch (Exception e)
        {
            if (e is not TaskCanceledException)
                _notificationService.CreateNotification().WithTitle("Login failed").WithMessage(e.Message).WithSeverity(NotificationSeverity.Error).Show();
            ContentDialog?.Hide(ContentDialogResult.Secondary);
        }
    }
}