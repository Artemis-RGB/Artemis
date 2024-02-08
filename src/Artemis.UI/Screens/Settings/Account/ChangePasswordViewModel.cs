using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Settings.Account;

public partial class ChangePasswordViewModel : ContentDialogViewModelBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly IWindowService _windowService;
    [Notify] private string _currentPassword = string.Empty;
    [Notify] private string _newPassword = string.Empty;
    [Notify] private string _newPasswordConfirmation = string.Empty;

    public ChangePasswordViewModel(IUserManagementService userManagementService, IWindowService windowService)
    {
        _userManagementService = userManagementService;
        _windowService = windowService;
        Submit = ReactiveCommand.CreateFromTask(ExecuteSubmit, ValidationContext.Valid);
        
        this.ValidationRule(vm => vm.CurrentPassword, e => !string.IsNullOrWhiteSpace(e), "You must specify your current password");
        this.ValidationRule(vm => vm.NewPassword, e => !string.IsNullOrWhiteSpace(e), "You must specify a new password");
        this.ValidationRule(
            vm => vm.NewPasswordConfirmation,
            this.WhenAnyValue(vm => vm.NewPassword, vm => vm.NewPasswordConfirmation, (password, confirmation) => password == confirmation),
            "The passwords must match"
        );
    }

    public ReactiveCommand<Unit, Unit> Submit { get; }

    private async Task ExecuteSubmit(CancellationToken cts)
    {
        ApiResult result = await _userManagementService.ChangePassword(CurrentPassword, NewPassword, cts);
        if (result.IsSuccess)
            await _windowService.ShowConfirmContentDialog("Password changed", "Your password has been changed", cancel: null);
        else
            await _windowService.ShowConfirmContentDialog("Failed to change password", result.Message ?? "An unexpected error occured", cancel: null);
    }
}