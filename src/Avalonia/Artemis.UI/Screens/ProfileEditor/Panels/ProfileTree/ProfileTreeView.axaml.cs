using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public partial class ProfileTreeView : ReactiveUserControl<ProfileTreeViewModel>
    {
        public ProfileTreeView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
