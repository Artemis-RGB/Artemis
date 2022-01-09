using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties
{
    public partial class ProfileElementPropertiesView : ReactiveUserControl<ProfileElementPropertiesViewModel>
    {
        public ProfileElementPropertiesView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
