using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Animations
{
    public class SlideLayerElement : LayerElement
    {
        public SlideLayerElement(Layer layer, LayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, settings, descriptor)
        {
        }

        public int MovePercentage { get; set; }

        public override LayerElementViewModel GetViewModel()
        {
            return null;
        }

        public override void Update(double deltaTime)
        {
            MovePercentage++;
            if (MovePercentage > 100)
                MovePercentage = 0;
        }

        public override SKShader RenderPostProcess(ArtemisSurface surface, SKBitmap bitmap, SKShader shader)
        {
            return SKShader.CreateLocalMatrix(shader, SKMatrix.MakeTranslation(Layer.RenderRectangle.Width / 100 * MovePercentage * -1, 0));
        }
    }
}