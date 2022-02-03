using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers
{
    public partial class LayerShapeVisualizerView : ReactiveUserControl<LayerShapeVisualizerViewModel>
    {
        public LayerShapeVisualizerView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
