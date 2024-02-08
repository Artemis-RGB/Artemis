using Artemis.UI.Shared.Extensions;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings.Account;

public partial class ChangeEmailAddressView : ReactiveUserControl<ChangeEmailAddressViewModel>
{
    public ChangeEmailAddressView()
    {
        InitializeComponent();
        this.WhenActivated(_ => this.ClearAllDataValidationErrors());
    }

}