using System;
using Artemis.Core.Models.Profile;
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

        public override void RenderPreProcess(SKPath framePath, SKCanvas canvas)
        {
            canvas.RotateDegrees(Rotation, Layer.RenderRectangle.MidX, Layer.RenderRectangle.MidY);
            framePath.Transform(SKMatrix.MakeRotationDegrees(Rotation * -1, Layer.RenderRectangle.MidX, Layer.RenderRectangle.MidY));
        }
    }
}