using Artemis.UI.Shared.Extensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
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