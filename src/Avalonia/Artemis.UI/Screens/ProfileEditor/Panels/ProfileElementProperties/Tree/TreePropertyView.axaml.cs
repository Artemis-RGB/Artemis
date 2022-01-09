using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Tree
{
    public partial class TreePropertyView : ReactiveUserControl<IActivatableViewModel>
    {
        public TreePropertyView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
