using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public partial class SidebarScreenView : ReactiveUserControl<SidebarScreenViewModel>
{
    public SidebarScreenView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        // if (ViewModel != null)
            // ViewModel.IsExpanded = !ViewModel.IsExpanded;
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        e.Handled = true;
    }
}