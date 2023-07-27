using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Media.Imaging;
using Flurl.Http;
using ReactiveUI;
using Serilog;

namespace Artemis.UI.Screens.Workshop.CurrentUser;

public class CurrentUserViewModel : ActivatableViewModelBase
{
    private readonly ILogger _logger;
    private readonly IAuthenticationService _authenticationService;
    private bool _loading = true;
    private Bitmap? _avatar;
    private string? _email;
    private string? _name;
    private string? _userId;

    public CurrentUserViewModel(ILogger logger, IAuthenticationService authenticationService)
    {
        _logger = logger;
        _authenticationService = authenticationService;
        Login = ReactiveCommand.CreateFromTask(ExecuteLogin);

        this.WhenActivated(d => ReactiveCommand.CreateFromTask(ExecuteAutoLogin).Execute().Subscribe().DisposeWith(d));
    }

    public bool Loading
    {
        get => _loading;
        set => RaiseAndSetIfChanged(ref _loading, value);
    }

    public string? UserId
    {
        get => _userId;
        set => RaiseAndSetIfChanged(ref _userId, value);
    }

    public string? Name
    {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string? Email
    {
        get => _email;
        set => RaiseAndSetIfChanged(ref _email, value);
    }

    public Bitmap? Avatar
    {
        get => _avatar;
        set => RaiseAndSetIfChanged(ref _avatar, value);
    }

    public ReactiveCommand<Unit, Unit> Login { get; }

    public void Logout()
    {
        _authenticationService.Logout();
    }

    private async Task ExecuteLogin(CancellationToken cancellationToken)
    {
        await _authenticationService.Login();
        await LoadCurrentUser();
    }

    private async Task ExecuteAutoLogin(CancellationToken cancellationToken)
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
        if (!_authenticationService.IsLoggedIn)
            return;

        UserId = _authenticationService.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        Name = _authenticationService.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        Email = _authenticationService.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

        if (UserId != null)
            await LoadAvatar(UserId);
    }

    private async Task LoadAvatar(string userId)
    {
        try
        {
            Avatar = new Bitmap(await $"{WorkshopConstants.AUTHORITY_URL}/user/avatar/{userId}".GetStreamAsync());
        }
        catch (Exception)
        {
            // ignored
        }
    }
}