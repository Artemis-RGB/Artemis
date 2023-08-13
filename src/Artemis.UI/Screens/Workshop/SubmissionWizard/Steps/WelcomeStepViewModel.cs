using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.WebClient.Workshop.Services;
using IdentityModel;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class WelcomeStepViewModel : SubmissionViewModel
{
    private readonly IAuthenticationService _authenticationService;

    public WelcomeStepViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;

        Continue = ReactiveCommand.CreateFromTask(ExecuteContinue);
        ShowHeader = false;
        ShowGoBack = false;
    }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> Continue { get; }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> GoBack { get; } = null!;

    private async Task ExecuteContinue()
    {
        bool loggedIn = await _authenticationService.AutoLogin(true);

        if (!loggedIn)
        {
            State.ChangeScreen<LoginStepViewModel>();
        }
        else
        {
            if (_authenticationService.Claims.Any(c => c.Type == JwtClaimTypes.EmailVerified && c.Value == "true"))
                State.ChangeScreen<EntryTypeStepViewModel>();
            else
                State.ChangeScreen<ValidateEmailStepViewModel>();
        }
    }
}