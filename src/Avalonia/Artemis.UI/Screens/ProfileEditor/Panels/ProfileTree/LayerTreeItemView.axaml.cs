using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public partial class LayerTreeItemView : UserControl
    {
        public LayerTreeItemView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
