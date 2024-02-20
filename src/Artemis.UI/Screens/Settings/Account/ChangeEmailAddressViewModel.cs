using System;
using System.ComponentModel.DataAnnotations;
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

public partial class ChangeEmailAddressViewModel : ContentDialogViewModelBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly IWindowService _windowService;
    [Notify] private string _emailAddress = string.Empty;

    public ChangeEmailAddressViewModel(IUserManagementService userManagementService, IAuthenticationService authenticationService, IWindowService windowService)
    {
        _userManagementService = userManagementService;
        _windowService = windowService;
        Submit = ReactiveCommand.CreateFromTask(ExecuteSubmit, ValidationContext.Valid);

        string? currentEmail = authenticationService.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Email)?.Value;
        this.ValidationRule(vm => vm.EmailAddress, e => !string.IsNullOrWhiteSpace(e),
            "You must specify a new email address");
        this.ValidationRule(vm => vm.EmailAddress, e => string.IsNullOrWhiteSpace(e) || new EmailAddressAttribute().IsValid(e),
            "You must specify a valid email address");
        this.ValidationRule(vm => vm.EmailAddress,
            e => string.IsNullOrWhiteSpace(e) || currentEmail == null || !string.Equals(e, currentEmail, StringComparison.InvariantCultureIgnoreCase),
            "New email address must be different from the old one");
    }

    public ReactiveCommand<Unit, Unit> Submit { get; }

    private async Task ExecuteSubmit(CancellationToken cts)
    {
        ApiResult result = await _userManagementService.ChangeEmailAddress(EmailAddress, cts);
        if (result.IsSuccess)
            await _windowService.ShowConfirmContentDialog("Confirmation required", "Before being applied, you must confirm your new email address. Please check your inbox.", cancel: null);
        else
            await _windowService.ShowConfirmContentDialog("Failed to update email address", result.Message ?? "An unexpected error occured", cancel: null);
    }
}