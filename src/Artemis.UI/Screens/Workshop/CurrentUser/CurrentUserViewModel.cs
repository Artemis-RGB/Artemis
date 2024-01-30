using System;
using System.Linq;
using System.Net.Http;
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
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using Serilog;

namespace Artemis.UI.Screens.Workshop.CurrentUser;

public partial class CurrentUserViewModel : ActivatableViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ObservableAsPropertyHelper<bool> _isAnonymous;
    private readonly ILogger _logger;
    private readonly IWindowService _windowService;
    [Notify] private bool _allowLogout = true;
    [Notify(Setter.Private)] private Bitmap? _avatar;
    [Notify(Setter.Private)] private string? _email;
    [Notify(Setter.Private)] private bool _loading = true;
    [Notify(Setter.Private)] private string? _name;
    [Notify(Setter.Private)] private string? _userId;
    [Notify(Setter.Private)] private string? _avatarUrl;

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
            _authenticationService.IsLoggedIn.Subscribe(_ => LoadCurrentUser()).DisposeWith(d);
        });
    }

    public bool IsAnonymous => _isAnonymous.Value;
    
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
            LoadCurrentUser();
    }

    private async Task AutoLogin()
    {
        try
        {
            await _authenticationService.AutoLogin();
            LoadCurrentUser();
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

    private void LoadCurrentUser()
    {
        UserId = _authenticationService.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        Name = _authenticationService.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        Email = _authenticationService.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        AvatarUrl = $"{WorkshopConstants.AUTHORITY_URL}/user/avatar/{UserId}";
    }
}