using Avalonia.Input;
using ReactiveUI.Avalonia;

namespace Artemis.UI.Screens.Sidebar;

public partial class SidebarProfileConfigurationView : ReactiveUserControl<SidebarProfileConfigurationViewModel>
{
    public SidebarProfileConfigurationView()
    {
        InitializeComponent();
    }

    private void ProfileContainerGrid_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // When right-clicking only open the context menu, don't select the profile
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            e.Handled = true;    
    }
}