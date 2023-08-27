using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public partial class SidebarView : ReactiveUserControl<SidebarViewModel>
{
    public SidebarView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Source is IDataContextProvider dataContextProvider && dataContextProvider.DataContext is SidebarScreenViewModel sidebarScreenViewModel)
            ViewModel?.NavigateToScreen(sidebarScreenViewModel);
    }
}