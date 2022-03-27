using Artemis.VisualScripting.Nodes.Easing.CustomViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Easing.CustomViews
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
