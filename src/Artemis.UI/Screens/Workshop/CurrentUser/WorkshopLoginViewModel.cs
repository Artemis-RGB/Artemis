using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
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
    private readonly INotificationService _notificationService;
    private readonly CancellationTokenSource _cts;
    private ObservableAsPropertyHelper<string?>? _deviceCode;
    private string? _uri;

    public WorkshopLoginViewModel(IAuthenticationService authenticationService, INotificationService notificationService)
    {
        _authenticationService = authenticationService;
        _notificationService = notificationService;
        _cts = new CancellationTokenSource();

        OpenBrowser = ReactiveCommand.Create(ExecuteOpenBrowser);
        CopyToClipboard = ReactiveCommand.CreateFromTask(ExecuteCopyToClipboard);

        this.WhenActivated(d =>
        {
            Dispatcher.UIThread.InvokeAsync(Login);

            _deviceCode = _authenticationService.UserCode
                .Select(uc => uc != null ? string.Join("-", string.Join("-", uc.Chunk(3).Select(c => new string(c)))) : null)
                .ToProperty(this, vm => vm.Code)
                .DisposeWith(d);
            _authenticationService.VerificationUri.Subscribe(u => _uri = u).DisposeWith(d);
            Disposable.Create(_cts, cts => cts.Cancel()).DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit> OpenBrowser { get; }
    public ReactiveCommand<Unit, Unit> CopyToClipboard { get; }

    public string? Code => _deviceCode?.Value;

    private async Task Login()
    {
        try
        {
            bool result = await _authenticationService.Login(_cts.Token);
            if (result)
                _notificationService.CreateNotification().WithTitle("Login succeeded").WithSeverity(NotificationSeverity.Success).Show();
            ContentDialog?.Hide(result ? ContentDialogResult.Primary : ContentDialogResult.Secondary);
        }
        catch (Exception e)
        {
            if (e is not TaskCanceledException)
                _notificationService.CreateNotification().WithTitle("Login failed").WithMessage(e.Message).WithSeverity(NotificationSeverity.Error).Show();
            ContentDialog?.Hide(ContentDialogResult.Secondary);
        }
    }

    private void ExecuteOpenBrowser()
    {
        if (_uri == null)
            return;
        Utilities.OpenUrl(_uri);
    }

    private async Task ExecuteCopyToClipboard()
    {
        if (Code == null)
            return;
        await Shared.UI.Clipboard.SetTextAsync(Code);
        _notificationService.CreateNotification().WithMessage("Code copied to clipboard.").Show();
    }
}