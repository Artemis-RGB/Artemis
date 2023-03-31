using Artemis.UI.Shared;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Input.Screens;

public partial class HotkeyToggleNodeCustomView : ReactiveUserControl<HotkeyToggleNodeCustomViewModel>
{
    public HotkeyToggleNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void HotkeyBox_OnHotkeyChanged(HotkeyBox sender, EventArgs args)
    {
        ViewModel?.Save();
    }
}