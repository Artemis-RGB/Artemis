using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.Device
{
    public partial class InputMappingsTabView : UserControl
    {
        public InputMappingsTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
