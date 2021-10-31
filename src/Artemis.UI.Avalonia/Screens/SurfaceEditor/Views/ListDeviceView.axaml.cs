using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Avalonia.Screens.SurfaceEditor.Views
{
    public partial class ListDeviceView : UserControl
    {
        public ListDeviceView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
