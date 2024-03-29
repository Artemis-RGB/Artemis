﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Settings.Account;
using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using IdentityModel;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public partial class AccountTabViewModel : RoutableScreen
{
    private readonly IWindowService _windowService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserManagementService _userManagementService;
    private ObservableAsPropertyHelper<bool>? _canChangePassword;

    [Notify(Setter.Private)] private string? _name;
    [Notify(Setter.Private)] private string? _email;
    [Notify(Setter.Private)] private string? _avatarUrl;
    [Notify(Setter.Private)] private ObservableCollection<PersonalAccessToken> _personalAccessTokens = new();

    public AccountTabViewModel(IWindowService windowService, IAuthenticationService authenticationService, IUserManagementService userManagementService)
    {
        _windowService = windowService;
        _authenticationService = authenticationService;
        _userManagementService = userManagementService;
        _authenticationService.AutoLogin(true);

        DisplayName = "Account";
        IsLoggedIn = _authenticationService.IsLoggedIn;
        DeleteToken = ReactiveCommand.CreateFromTask<PersonalAccessToken>(ExecuteDeleteToken);

        this.WhenActivated(d =>
        {
            _canChangePassword = _authenticationService.GetClaim(JwtClaimTypes.AuthenticationMethod).Select(c => c?.Value == "pwd").ToProperty(this, vm => vm.CanChangePassword);
            _canChangePassword.DisposeWith(d);
        });
        this.WhenActivated(d => _authenticationService.IsLoggedIn.Subscribe(_ => LoadCurrentUser()).DisposeWith(d));
    }

    public bool CanChangePassword => _canChangePassword?.Value ?? false;
    public IObservable<bool> IsLoggedIn { get; }
    public ReactiveCommand<PersonalAccessToken,Unit> DeleteToken { get; }

    public async Task Login()
    {
        await _windowService.CreateContentDialog().WithViewModel(out WorkshopLoginViewModel _).WithTitle("Account login").ShowAsync();
    }

    public async Task ChangeAvatar()
    {
        string[]? result = await _windowService.CreateOpenFileDialog().HavingFilter(f => f.WithBitmaps()).ShowAsync();
        if (result == null)
            return;

        try
        {
            AvatarUrl = $"{WorkshopConstants.AUTHORITY_URL}/user/avatar/{Guid.Empty}";
            await using FileStream fileStream = new(result.First(), FileMode.Open);
            ApiResult changeResult = await _userManagementService.ChangeAvatar(fileStream, CancellationToken.None);
            if (!changeResult.IsSuccess)
                await _windowService.ShowConfirmContentDialog("Failed to change image", changeResult.Message ?? "An unexpected error occured", cancel: null);
        }
        finally
        {
            string? userId = _authenticationService.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            AvatarUrl = $"{WorkshopConstants.AUTHORITY_URL}/user/avatar/{userId}";
        }
    }

    public async Task ChangeEmailAddress()
    {
        await _windowService.CreateContentDialog().WithTitle("Change email address")
            .WithViewModel(out ChangeEmailAddressViewModel vm)
            .WithCloseButtonText("Cancel")
            .HavingPrimaryButton(b => b.WithText("Submit").WithCommand(vm.Submit))
            .ShowAsync();
    }

    public async Task ChangePasswordAddress()
    {
        await _windowService.CreateContentDialog().WithTitle("Change password")
            .WithViewModel(out ChangePasswordViewModel vm)
            .WithCloseButtonText("Cancel")
            .HavingPrimaryButton(b => b.WithText("Submit").WithCommand(vm.Submit))
            .ShowAsync();
    }

    public async Task RemoveAccount()
    {
        await _windowService.CreateContentDialog().WithTitle("Remove account")
            .WithViewModel(out RemoveAccountViewModel vm)
            .WithCloseButtonText("Cancel")
            .HavingPrimaryButton(b => b.WithText("Permanently remove account").WithCommand(vm.Submit))
            .ShowAsync();
    }

    public async Task GenerateToken()
    {
        await _windowService.CreateContentDialog().WithTitle("Create Personal Access Token")
            .WithViewModel(out CreatePersonalAccessTokenViewModel vm)
            .WithCloseButtonText("Cancel")
            .HavingPrimaryButton(b => b.WithText("Submit").WithCommand(vm.Submit))
            .ShowAsync();
        
        List<PersonalAccessToken> personalAccessTokens = await _userManagementService.GetPersonAccessTokens(CancellationToken.None);
        PersonalAccessTokens = new ObservableCollection<PersonalAccessToken>(personalAccessTokens);
    }

    private async Task ExecuteDeleteToken(PersonalAccessToken token)
    {
        bool confirm = await _windowService.ShowConfirmContentDialog("Delete Personal Access Token", "Are you sure you want to delete this token? Any services using it will stop working");
        if (!confirm)
            return;

        await _userManagementService.DeletePersonAccessToken(token, CancellationToken.None);
        PersonalAccessTokens.Remove(token);
    }

    private async Task LoadCurrentUser()
    {
        string? userId = _authenticationService.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        Name = _authenticationService.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        Email = _authenticationService.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        AvatarUrl = $"{WorkshopConstants.AUTHORITY_URL}/user/avatar/{userId}";

        List<PersonalAccessToken> personalAccessTokens = await _userManagementService.GetPersonAccessTokens(CancellationToken.None);
        PersonalAccessTokens = new ObservableCollection<PersonalAccessToken>(personalAccessTokens);
    }
}