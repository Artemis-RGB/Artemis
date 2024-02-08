using System;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Services;
using IdentityModel;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Settings.Account;

public partial class RemoveAccountViewModel : ContentDialogViewModelBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IWindowService _windowService;
    [Notify] private string _emailAddress = string.Empty;

    public RemoveAccountViewModel(IUserManagementService userManagementService, IAuthenticationService authenticationService, IWindowService windowService)
    {
        _userManagementService = userManagementService;
        _authenticationService = authenticationService;
        _windowService = windowService;

        Submit = ReactiveCommand.CreateFromTask(ExecuteSubmit, ValidationContext.Valid);
        string? currentEmail = authenticationService.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Email)?.Value;
        
        this.ValidationRule(vm => vm.EmailAddress, e => !string.IsNullOrWhiteSpace(e), "You must enter your email address");
        this.ValidationRule(vm => vm.EmailAddress,
            e => string.IsNullOrWhiteSpace(e) || string.Equals(e, currentEmail, StringComparison.InvariantCultureIgnoreCase),
            "The entered email address is not correct");
    }

    public ReactiveCommand<Unit, Unit> Submit { get; }

    private async Task ExecuteSubmit(CancellationToken cts)
    {
        ApiResult result = await _userManagementService.RemoveAccount(cts);
        if (result.IsSuccess)
        {
            await _windowService.ShowConfirmContentDialog("Account removed", "Hopefully we'll see you again!", cancel: null);
            _authenticationService.Logout();
        }
        else
            await _windowService.ShowConfirmContentDialog("Failed to remove account", result.Message ?? "An unexpected error occured", cancel: null);
    }
}