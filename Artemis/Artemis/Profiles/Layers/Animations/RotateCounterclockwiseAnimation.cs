using System;
using System.Windows.Media;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Animations
{
    public class RotateCounterclockwiseAnimation : ILayerAnimation
    {
        public string Name => "Rotate counterclockwise";

        public void Update(LayerModel layerModel, bool updateAnimations)
        {
            var progress = layerModel.AnimationProgress;

            if (MustExpire(layerModel))
                progress = 0;
            progress = progress + layerModel.Properties.AnimationSpeed / 2.5;

            // If not previewing, store the animation progress in the actual model for the next frame
            if (updateAnimations)
                layerModel.AnimationProgress = progress;
        }

        public void Draw(LayerModel layerModel, DrawingContext c, int drawScale)
        {
            if (layerModel.Brush == null)
                return;

            // Set up variables for this frame
            var fillRect = layerModel.Properties.PropertiesRect(drawScale);
            var fillSize = Math.Sqrt(fillRect.Width * fillRect.Width + fillRect.Height * fillRect.Height);
            fillRect.Inflate(fillSize - fillRect.Width, fillSize - fillRect.Height);
            
            var clip = layerModel.LayerRect(drawScale);

            c.PushClip(new RectangleGeometry(clip));
            c.PushTransform(new RotateTransform(36 * layerModel.AnimationProgress*-1, fillRect.X + fillRect.Width / 2, fillRect.Y + fillRect.Height / 2));
            c.DrawRectangle(layerModel.Brush, null, fillRect);
            c.Pop();
            c.Pop();
        }

        public bool MustExpire(LayerModel layer) => layer.AnimationProgress >= 10;
    }
}
