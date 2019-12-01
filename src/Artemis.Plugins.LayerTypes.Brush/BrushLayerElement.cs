using System.Drawing;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerElement;

namespace Artemis.Plugins.LayerElements.Brush
{
    public class BrushLayerElement : LayerElement
    {
        public BrushLayerElement(Layer layer, BrushLayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, settings, descriptor)
        {
            Settings = settings ?? new BrushLayerElementSettings {Brush = new SolidBrush(Color.Red)};
        }

        public new BrushLayerElementSettings Settings { get; }

        public override LayerElementViewModel GetViewModel()
        {
            return new BrushLayerElementViewModel(this);
        }

        public override void Update(double deltaTime)
        {
        }

        public override void RenderPreProcess(ArtemisSurface surface, Graphics graphics)
        {
        }

        public override void Render(ArtemisSurface surface, Graphics graphics)
        {
            if (Settings?.Brush != null)
                graphics.FillRectangle(Settings.Brush, Layer.RenderRectangle);
        }

        public override void RenderPostProcess(ArtemisSurface surface, Graphics graphics)
        {
        }
    }
}