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

namespace Artemis.UI.Screens.Workshop.CurrentUser;

public class CurrentUserViewModel : ActivatableViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private ObservableAsPropertyHelper<bool>? _isLoggedIn;

    private string? _userId;
    private string? _name;
    private string? _email;
    private Bitmap? _avatar;

    public CurrentUserViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        Login = ReactiveCommand.CreateFromTask(ExecuteLogin);

        this.WhenActivated(d => _isLoggedIn = _authenticationService.WhenAnyValue(s => s.IsLoggedIn).ToProperty(this, vm => vm.IsLoggedIn).DisposeWith(d));
        this.WhenActivated(d =>
        {
            Task.Run(async () =>
            {
                await _authenticationService.AutoLogin();
                await LoadCurrentUser();
            }).DisposeWith(d);
        });
    }

    public void Logout()
    {
        _authenticationService.Logout();
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
    public bool IsLoggedIn => _isLoggedIn?.Value ?? false;

    private async Task ExecuteLogin(CancellationToken cancellationToken)
    {
        await _authenticationService.Login();
        await LoadCurrentUser();
        Console.WriteLine(_authenticationService.Claims);
    }

    private async Task LoadCurrentUser()
    {
        if (!IsLoggedIn)
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