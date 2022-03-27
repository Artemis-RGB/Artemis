using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.VisualScripting.Nodes.Easing.CustomViews
{
    public partial class NodeEasingView : UserControl
    {
        public NodeEasingView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
