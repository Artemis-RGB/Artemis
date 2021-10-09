using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Avalonia.Screens.Root.Views
{
    public class SidebarProfileConfigurationView : UserControl
    {
        public SidebarProfileConfigurationView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}