using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.VisualScripting.Nodes.CustomViews
{
    public partial class StaticStringValueNodeCustomView : UserControl
    {
        public StaticStringValueNodeCustomView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
