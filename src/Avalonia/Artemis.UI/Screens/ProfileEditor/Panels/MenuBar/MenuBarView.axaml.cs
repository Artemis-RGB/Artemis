using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.ProfileEditor.Panels.MenuBar
{
    public partial class MenuBarView : UserControl
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
