using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Avalonia.Screens.Root.Views
{
    public class SidebarView : UserControl
    {
        public SidebarView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}