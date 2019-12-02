using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Brush
{
    public class BrushLayerElement : LayerElement
    {
        public BrushLayerElement(Layer layer, BrushLayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, settings, descriptor)
        {
            Settings = settings ?? new BrushLayerElementSettings();
        }

        public new BrushLayerElementSettings Settings { get; }

        public override LayerElementViewModel GetViewModel()
        {
            return new BrushLayerElementViewModel(this);
        }

        public override void Update(double deltaTime)
        {
        }

        public override void RenderPreProcess(ArtemisSurface surface, SKCanvas canvas)
        {
        }

        public override void Render(ArtemisSurface surface, SKCanvas canvas)
        {
            canvas.DrawRect(Layer.RenderRectangle, new SKPaint {Color = new SKColor(255, 255, 255, 255)});
        }

        public override void RenderPostProcess(ArtemisSurface surface, SKCanvas canvas)
        {
        }
    }
}