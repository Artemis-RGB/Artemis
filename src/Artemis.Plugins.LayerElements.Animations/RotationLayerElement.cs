using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Animations
{
    public class RotationLayerElement : LayerElement
    {
        public RotationLayerElement(Layer layer, Guid guid, LayerElementSettings settings, LayerElementDescriptor descriptor) : base(layer, guid, settings, descriptor)
        {
        }

        public float Rotation { get; set; }

        public override LayerElementViewModel GetViewModel()
        {
            return null;
        }

        public override void Update(double deltaTime)
        {
            Rotation += (float) (deltaTime * 100);
            if (Rotation > 360)
                Rotation = 0;
        }

        public override SKShader RenderPostProcess(ArtemisSurface surface, SKBitmap bitmap, SKShader shader)
        {
            var rect = Layer.AbsoluteRenderRectangle;
            var center = new SKPoint(rect.MidX, rect.MidY);
            
            var required = (float) Math.Sqrt(rect.Width * rect.Width + rect.Height * rect.Height);
            var minSide = Math.Min(rect.Width, rect.Height);
            var scale = required / minSide;
            return SKShader.CreateLocalMatrix(SKShader.CreateLocalMatrix(shader, SKMatrix.MakeScale(scale, scale, center.X, center.Y)), SKMatrix.MakeRotationDegrees(Rotation, center.X, center.Y));
        }
    }
}