using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Controls;
using Flurl.Http;
using ReactiveUI;
using Serilog;

namespace Artemis.UI.Screens.Workshop.CurrentUser;

public class CurrentUserViewModel : ActivatableViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ObservableAsPropertyHelper<bool> _isAnonymous;
    private readonly ILogger _logger;
    private readonly IWindowService _windowService;
    private bool _allowLogout = true;
    private Bitmap? _avatar;
    private string? _email;
    private bool _loading = true;
    private string? _name;
    private string? _userId;

    public CurrentUserViewModel(ILogger logger, IAuthenticationService authenticationService, IWindowService windowService)
    {
        _logger = logger;
        _authenticationService = authenticationService;
        _windowService = windowService;
        Login = ReactiveCommand.CreateFromTask(ExecuteLogin);

        _isAnonymous = this.WhenAnyValue(vm => vm.Loading, vm => vm.Name, (l, n) => l || n == null).ToProperty(this, vm => vm.IsAnonymous);

        this.WhenActivated(d =>
        {
            Task.Run(AutoLogin);
            _authenticationService.IsLoggedIn.Subscribe(_ => Task.Run(LoadCurrentUser)).DisposeWith(d);
        });
    }

    public bool IsAnonymous => _isAnonymous.Value;

    public bool AllowLogout
    {
        get => _allowLogout;
        set => RaiseAndSetIfChanged(ref _allowLogout, value);
    }

    public bool Loading
    {
        get => _loading;
        private set => RaiseAndSetIfChanged(ref _loading, value);
    }

    public string? UserId
    {
        get => _userId;
        private set => RaiseAndSetIfChanged(ref _userId, value);
    }

    public string? Name
    {
        get => _name;
        private set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string? Email
    {
        get => _email;
        private set => RaiseAndSetIfChanged(ref _email, value);
    }

    public Bitmap? Avatar
    {
        get => _avatar;
        private set => RaiseAndSetIfChanged(ref _avatar, value);
    }

    public ReactiveCommand<Unit, Unit> Login { get; }

    public void Logout()
    {
        if (AllowLogout)
            _authenticationService.Logout();
    }

    private async Task ExecuteLogin(CancellationToken cancellationToken)
    {
        ContentDialogResult result = await _windowService.CreateContentDialog()
            .WithViewModel(out WorkshopLoginViewModel _)
            .WithTitle("Workshop login")
            .ShowAsync();

        if (result == ContentDialogResult.Primary)
            await LoadCurrentUser();
    }

    private async Task AutoLogin()
    {
        try
        {
            await _authenticationService.AutoLogin();
            await LoadCurrentUser();
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to load the current user");
        }
        finally
        {
            Loading = false;
        }
    }

    private async Task LoadCurrentUser()
    {
        UserId = _authenticationService.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        Name = _authenticationService.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        Email = _authenticationService.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

        if (UserId != null)
        {
            await LoadAvatar(UserId);
        }
        else
        {
            Avatar?.Dispose();
            Avatar = null;
        }
    }

    private async Task LoadAvatar(string userId)
    {
        try
        {
            Avatar?.Dispose();
            Avatar = new Bitmap(await $"{WorkshopConstants.AUTHORITY_URL}/user/avatar/{userId}".GetStreamAsync());
        }
        catch (Exception)
        {
            // ignored
        }
    }
}