using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class ProfileEditorView : ReactiveUserControl<ProfileEditorViewModel>
    {
        public ProfileEditorView()
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