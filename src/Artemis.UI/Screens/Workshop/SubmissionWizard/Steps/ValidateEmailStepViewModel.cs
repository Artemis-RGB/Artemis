using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Artemis.WebClient.Workshop.Services;
using IdentityModel;
using ReactiveUI;
using Timer = System.Timers.Timer;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class ValidateEmailStepViewModel : SubmissionViewModel
{
    private readonly IAuthenticationService _authenticationService;
    private ObservableAsPropertyHelper<Claim?>? _email;

    public ValidateEmailStepViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;

        Continue = ReactiveCommand.Create(ExecuteContinue);
        Refresh = ReactiveCommand.CreateFromTask(ExecuteRefresh);
        ShowGoBack = false;
        ShowHeader = false;

        this.WhenActivated(d =>
        {
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

    public ReactiveCommand<Unit, Unit> Refresh { get; }

    public Claim? Email => _email?.Value;

    private async Task Update()
    {
        try
        {
            // Use the refresh token to login again, updating claims
            await _authenticationService.AutoLogin(true);

            Claim? emailVerified = _authenticationService.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.EmailVerified);
            if (emailVerified?.Value == "true")
                ExecuteContinue();
        }
        catch (Exception)
        {
            // ignored, meh
        }
    }

    private void ExecuteContinue()
    {
        State.ChangeScreen<EntryTypeViewModel>();
    }

    private async Task ExecuteRefresh(CancellationToken ct)
    {
        await Update();
        await Task.Delay(1000, ct);
    }
}