using Avalonia.Interactivity;
using ReactiveUI.Avalonia;

namespace Artemis.UI.Screens.Workshop.CurrentUser;

public partial class CurrentUserView : ReactiveUserControl<CurrentUserViewModel>
{
    public CurrentUserView()
    {
        InitializeComponent();
    }

    private void Signout_OnClick(object? sender, RoutedEventArgs e)
    {
        UserMenu.ContextFlyout?.Hide();
        ViewModel?.Logout();
    }
    
    private void Manage_OnClick(object? sender, RoutedEventArgs e)
    {
        UserMenu.ContextFlyout?.Hide();
        ViewModel?.ManageAccount();
    }
}