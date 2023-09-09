using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.WebClient.Workshop;
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
        Resend = ReactiveCommand.Create(() => Utilities.OpenUrl(WorkshopConstants.AUTHORITY_URL + "/account/confirm/resend"));

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

    public ReactiveCommand<Unit, Unit> Refresh { get; }
    public ReactiveCommand<Unit, Process?> Resend { get; }

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
        State.ChangeScreen<EntryTypeStepViewModel>();
    }

    private async Task ExecuteRefresh(CancellationToken ct)
    {
        await Update();
        await Task.Delay(1000, ct);
    }
}