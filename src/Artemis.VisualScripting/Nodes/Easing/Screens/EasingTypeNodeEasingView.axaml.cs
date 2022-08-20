using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.VisualScripting.Nodes.Easing.Screens
{
    public partial class EasingTypeNodeEasingView : UserControl
    {
        public EasingTypeNodeEasingView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
