using Stylet;

namespace Artemis.Core.Plugins.LayerBrush
{
    public abstract class LayerBrushViewModel : PropertyChangedBase
    {
        protected LayerBrushViewModel(LayerBrush brush)
        {
            Brush = brush;
        }

        public LayerBrush Brush { get; }
    }
}