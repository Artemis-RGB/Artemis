using Artemis.UI.Shared.Extensions;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings.Account;

public partial class ChangePasswordView : ReactiveUserControl<ChangePasswordViewModel>
{
    public ChangePasswordView()
    {
        InitializeComponent();
        this.WhenActivated(_ => this.ClearAllDataValidationErrors());
    }
}