using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Panels.MenuBar
{
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

        private void MenuItem_OnSubmenuOpened(object? sender, RoutedEventArgs e)
        {
            
        }
    }
}
