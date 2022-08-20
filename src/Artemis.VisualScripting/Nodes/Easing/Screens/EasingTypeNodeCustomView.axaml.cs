using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Easing.Screens
{
    public partial class EasingTypeNodeCustomView : ReactiveUserControl<EasingTypeNodeCustomViewModel>
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
