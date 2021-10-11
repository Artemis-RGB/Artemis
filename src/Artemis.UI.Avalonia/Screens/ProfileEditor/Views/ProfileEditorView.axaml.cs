using Artemis.UI.Avalonia.Screens.ProfileEditor.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.ProfileEditor.Views
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
    }
}