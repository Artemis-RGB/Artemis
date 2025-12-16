using Artemis.UI.Shared.Extensions;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings.Account;

public partial class CreatePersonalAccessTokenView : ReactiveUserControl<CreatePersonalAccessTokenViewModel>
{
    public CreatePersonalAccessTokenView()
    {
        InitializeComponent();
        this.WhenActivated(_ => this.ClearAllDataValidationErrors());
    }
}