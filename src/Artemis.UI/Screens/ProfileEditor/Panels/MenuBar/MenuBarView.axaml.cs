using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI.Avalonia;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.ProfileEditor.MenuBar;

public partial class MenuBarView : ReactiveUserControl<MenuBarViewModel>
{
    public MenuBarView()
    {
        InitializeComponent();
    }


    private void MenuBase_OnMenuClosed(object? sender, RoutedEventArgs e)
    {
        ProfileEditorView? profileEditorView = this.FindAncestorOfType<Window>().FindDescendantOfType<ProfileEditorView>();
        profileEditorView?.Focus();
    }
}