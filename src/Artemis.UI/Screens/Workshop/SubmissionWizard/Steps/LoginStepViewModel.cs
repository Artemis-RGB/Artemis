using System;
using System.Linq;
using System.Reactive;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Services;
using FluentAvalonia.UI.Controls;
using IdentityModel;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class LoginStepViewModel : SubmissionViewModel
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IWindowService _windowService;

    public LoginStepViewModel(IAuthenticationService authenticationService, IWindowService windowService)
    {
        _authenticationService = authenticationService;
        _windowService = windowService;

        Continue = ReactiveCommand.CreateFromTask(ExecuteLogin);
        ShowGoBack = false;
        ShowHeader = false;
        ContinueText = "Log In";
    }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> Continue { get; }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> GoBack { get; } = null!;

    private async Task ExecuteLogin(CancellationToken ct)
    {
        ContentDialogResult result = await _windowService.CreateContentDialog().WithViewModel(out WorkshopLoginViewModel _).WithTitle("Workshop login").ShowAsync();
        if (result != ContentDialogResult.Primary)
            return;

        Claim? emailVerified = _authenticationService.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.EmailVerified);
        if (emailVerified?.Value == "true")
            State.ChangeScreen<EntryTypeViewModel>();
        else
            State.ChangeScreen<ValidateEmailStepViewModel>();
    }
}