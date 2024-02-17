using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Settings.Account;

public partial class CreatePersonalAccessTokenViewModel : ContentDialogViewModelBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly IWindowService _windowService;
    [Notify] private string _description = string.Empty;
    [Notify] private DateTime _expirationDate = DateTime.UtcNow.Date.AddDays(181);

    public CreatePersonalAccessTokenViewModel(IUserManagementService userManagementService, IWindowService windowService)
    {
        _userManagementService = userManagementService;
        _windowService = windowService;
        Submit = ReactiveCommand.CreateFromTask(ExecuteSubmit, ValidationContext.Valid);

        this.ValidationRule(vm => vm.Description, e => e != null, "You must specify a description");
        this.ValidationRule(vm => vm.Description, e => e == null || e.Length >= 5, "You must specify a description of at least 5 characters");
        this.ValidationRule(vm => vm.Description, e => e == null || e.Length <= 150, "You must specify a description of less than 150 characters");
        this.ValidationRule(vm => vm.ExpirationDate, e => e >= DateTime.UtcNow.Date.AddDays(1), "Expiration date must be at least 24 hours from now");
    }

    public DateTime StartDate => DateTime.UtcNow.Date.AddDays(1);
    public DateTime EndDate => DateTime.UtcNow.Date.AddDays(365);
    public ReactiveCommand<Unit, Unit> Submit { get; }

    private async Task ExecuteSubmit(CancellationToken cts)
    {
        string result = await _userManagementService.CreatePersonAccessToken(Description, ExpirationDate, cts);
        await _windowService.CreateContentDialog()
            .WithTitle("Personal Access Token")
            .WithViewModel(out PersonalAccessTokenViewModel _, result)
            .WithFullScreen()
            .ShowAsync();
    }
}