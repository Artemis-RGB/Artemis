using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.CurrentUser;

public partial class CurrentUserView : ReactiveUserControl<CurrentUserViewModel>
{
    public CurrentUserView()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        UserMenu.ContextFlyout?.Hide();
        ViewModel?.Logout();
    }
}