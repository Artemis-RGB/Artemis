using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.ProfileEditor.MenuBar;

public partial class MenuBarView : ReactiveUserControl<MenuBarViewModel>
{
    public MenuBarView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void MenuBase_OnMenuClosed(object? sender, RoutedEventArgs e)
    {
        ProfileEditorView? profileEditorView = this.FindAncestorOfType<Window>().FindDescendantOfType<ProfileEditorView>();
        profileEditorView?.Focus();
    }
}