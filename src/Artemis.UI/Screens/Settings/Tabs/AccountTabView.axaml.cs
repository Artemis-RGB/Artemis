using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public partial class AccountTabView : ReactiveUserControl<AccountTabViewModel>
{
    public AccountTabView()
    {
        InitializeComponent();
    }
}