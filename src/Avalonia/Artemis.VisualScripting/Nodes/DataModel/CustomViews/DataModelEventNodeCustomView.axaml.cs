using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.VisualScripting.Nodes.DataModel.CustomViews
{
    public partial class DataModelEventNodeCustomView : UserControl
    {
        public DataModelEventNodeCustomView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
