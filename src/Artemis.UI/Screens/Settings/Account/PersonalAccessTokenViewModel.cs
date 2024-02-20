using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Settings.Account;

public class PersonalAccessTokenViewModel : ContentDialogViewModelBase
{
    public string Token { get; }

    public PersonalAccessTokenViewModel(string token)
    {
        Token = token;
    }
}