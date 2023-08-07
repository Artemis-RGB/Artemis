using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Services;
using IdentityModel;
using ReactiveUI;
using Timer = System.Timers.Timer;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class WelcomeStepViewModel : SubmissionViewModel
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ObservableAsPropertyHelper<bool> _showMissingVerification;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<Claim?>? _email;
    private ObservableAsPropertyHelper<Claim?>? _emailVerified;
    private ObservableAsPropertyHelper<bool>? _isLoggedIn;

    public WelcomeStepViewModel(IAuthenticationService authenticationService, IWindowService windowService)
    {
        _authenticationService = authenticationService;
        _windowService = windowService;
        _showMissingVerification = this.WhenAnyValue(vm => vm.IsLoggedIn, vm => vm.EmailVerified, (l, v) => l && (v == null || v.Value == "false")).ToProperty(this, vm => vm.ShowMissingVerification);

        ShowGoBack = false;
        Continue = ReactiveCommand.Create(ExecuteContinue);
        Login = ReactiveCommand.CreateFromTask(ExecuteLogin);
        Refresh = ReactiveCommand.CreateFromTask(ExecuteRefresh);

        this.WhenActivated(d =>
        {
            _isLoggedIn = authenticationService.IsLoggedIn.ToProperty(this, vm => vm.IsLoggedIn).DisposeWith(d);
            _emailVerified = authenticationService.GetClaim(JwtClaimTypes.EmailVerified).ToProperty(this, vm => vm.EmailVerified).DisposeWith(d);
            _email = authenticationService.GetClaim(JwtClaimTypes.Email).ToProperty(this, vm => vm.Email).DisposeWith(d);

            Timer updateTimer = new(TimeSpan.FromSeconds(15));
            updateTimer.Elapsed += (_, _) => Task.Run(Update);
            updateTimer.Start();

            updateTimer.DisposeWith(d);
        });
    }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> Continue { get; }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> GoBack { get; } = null!;

    public ReactiveCommand<Unit, Unit> Login { get; }
    public ReactiveCommand<Unit, Unit> Refresh { get; }

    public bool ShowMissingVerification => _showMissingVerification.Value;
    public bool IsLoggedIn => _isLoggedIn?.Value ?? false;
    public Claim? EmailVerified => _emailVerified?.Value;
    public Claim? Email => _email?.Value;

    private async Task Update()
    {
        if (EmailVerified?.Value == "true")
            return;

        try
        {
            // Use the refresh token to login again, updating claims
            await _authenticationService.AutoLogin(true);
        }
        catch (Exception)
        {
            // ignored, meh
        }
    }

    private void ExecuteContinue()
    {
        throw new NotImplementedException();
    }

    private async Task ExecuteLogin(CancellationToken ct)
    {
        await _windowService.CreateContentDialog().WithViewModel(out WorkshopLoginViewModel _).WithTitle("Workshop login").ShowAsync();
    }

    private async Task ExecuteRefresh(CancellationToken ct)
    {
        await Update();
        await Task.Delay(1000, ct);
    }
}