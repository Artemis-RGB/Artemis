using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Animations
{
    public class SlideLayerElement : LayerElement
    {
        public SlideLayerElement(Layer layer, Guid guid, BrushSettings settings, BrushDescriptor descriptor) : base(layer, guid, settings, descriptor)
        {
        }

        public int MovePercentage { get; set; }

        public override BrushViewModel GetViewModel()
        {
            return null;
        }

        public override void Update(double deltaTime)
        {
            MovePercentage++;
            if (MovePercentage > 100)
                MovePercentage = 0;
        }

        public override void RenderPreProcess(SKPath framePath, SKCanvas canvas)
        {
            canvas.Translate(Layer.RenderRectangle.Width / 100 * MovePercentage * -1, 0);
            framePath.Transform(SKMatrix.MakeTranslation(Layer.RenderRectangle.Width / 100 * MovePercentage, 0));
        }
    }
}