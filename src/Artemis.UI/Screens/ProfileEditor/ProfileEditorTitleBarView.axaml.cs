using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.ProfileEditor
{
    public partial class ProfileEditorTitleBarView : UserControl
    {
        public ProfileEditorTitleBarView()
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