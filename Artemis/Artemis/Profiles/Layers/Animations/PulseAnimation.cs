using System;
using System.Windows.Media;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Animations
{
    public class PulseAnimation : ILayerAnimation
    {
        public string Name => "Pulse";

        public void Update(LayerModel layerModel, bool updateAnimations)
        {
            // TODO: Generic implementation
            // Reset animation progress if layer wasn't drawn for 100ms
            if ((new TimeSpan(0, 0, 0, 0, 100) < DateTime.Now - layerModel.LastRender) && updateAnimations)
                layerModel.AnimationProgress = 0;

            var progress = layerModel.AnimationProgress;

            if (MustExpire(layerModel))
                progress = 0;
            progress = progress + layerModel.Properties.AnimationSpeed/2;

            // If not previewing, store the animation progress in the actual model for the next frame
            if (updateAnimations)
                layerModel.AnimationProgress = progress;
        }

        public void Draw(LayerModel layerModel, DrawingContext c, int drawScale)
        {
            if (layerModel.Brush == null)
                return;

            // Set up variables for this frame
            var rect = layerModel.Properties.Contain
                ? layerModel.LayerRect(drawScale)
                : layerModel.Properties.PropertiesRect(drawScale);

            var clip = layerModel.LayerRect(drawScale);

            // Can't meddle with the original brush because it's frozen.
            var brush = layerModel.Brush.Clone();
            brush.Opacity = (Math.Sin(layerModel.AnimationProgress*Math.PI) + 1)*(layerModel.Opacity/2);
            layerModel.Brush = brush;

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(layerModel.Brush, null, rect);
            c.Pop();
        }

        public bool MustExpire(LayerModel layer) => layer.AnimationProgress > 2;
    }
}