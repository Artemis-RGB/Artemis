using System.Threading.Tasks;
using Artemis.WebClient.Workshop.Services;
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

    private async Task ExecuteContinue()
    {
        bool loggedIn = await _authenticationService.AutoLogin(true);

        if (!loggedIn)
        {
            State.ChangeScreen<LoginStepViewModel>();
        }
        else
        {
            if (_authenticationService.GetIsEmailVerified())
                State.ChangeScreen<EntryTypeStepViewModel>();
            else
                State.ChangeScreen<ValidateEmailStepViewModel>();
        }
    }
}