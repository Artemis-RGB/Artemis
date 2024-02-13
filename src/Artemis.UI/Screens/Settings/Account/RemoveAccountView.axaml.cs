using Artemis.UI.Shared.Extensions;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings.Account;

public partial class RemoveAccountView : ReactiveUserControl<RemoveAccountViewModel>
{
    public RemoveAccountView()
    {
        InitializeComponent();
        this.WhenActivated(_ => this.ClearAllDataValidationErrors());
    }

}