using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers
{
    public partial class LayerVisualizerView : ReactiveUserControl<LayerVisualizerViewModel>
    {
        public LayerVisualizerView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
