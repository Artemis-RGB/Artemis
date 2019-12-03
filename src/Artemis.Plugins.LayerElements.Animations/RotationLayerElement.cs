using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Animations
{
    public class RotationLayerElement : LayerElement
    {
        public RotationLayerElement(Layer layer, LayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, settings, descriptor)
        {
        }

        public float Rotation { get; set; }

        public override LayerElementViewModel GetViewModel()
        {
            return null;
        }

        public override void Update(double deltaTime)
        {
            Rotation += (float)(deltaTime * 100);
            if (Rotation > 360)
                Rotation = 0;
        }

        public override SKShader RenderPostProcess(ArtemisSurface surface, SKBitmap bitmap, SKShader shader)
        {
            var center = new SKPoint(Layer.AbsoluteRenderRectangle.MidX, Layer.AbsoluteRenderRectangle.MidY);

            // TODO Scale so that the rectangle is covered in every rotation, instead of just putting it at 2
            return SKShader.CreateLocalMatrix(SKShader.CreateLocalMatrix(shader, SKMatrix.MakeScale(2, 2, center.X, center.Y)), SKMatrix.MakeRotationDegrees(Rotation, center.X, center.Y));
        }
    }
}