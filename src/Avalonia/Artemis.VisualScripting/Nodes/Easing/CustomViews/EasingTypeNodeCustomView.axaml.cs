using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.VisualScripting.Nodes.Easing.CustomViews
{
    public partial class EasingTypeNodeCustomView : UserControl
    {
        public EasingTypeNodeCustomView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
