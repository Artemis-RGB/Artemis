using System;
using System.Windows;
using System.Windows.Media;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Animations
{
    public class PulseAnimation : ILayerAnimation
    {
        public string Name { get; } = "Pulse";

        public void Update(LayerModel layerModel, bool updateAnimations)
        {
            // TODO: Generic implementation
            // Reset animation progress if layer wasn't drawn for 100ms
            if ((new TimeSpan(0, 0, 0, 0, 100) < DateTime.Now - layerModel.LastRender) && updateAnimations)
                layerModel.Properties.AnimationProgress = 0;

            var progress = layerModel.Properties.AnimationProgress;

            if (MustExpire(layerModel))
                progress = 0;
            progress = progress + layerModel.Properties.AnimationSpeed/2;

            // If not previewing, store the animation progress in the actual model for the next frame
            if (updateAnimations)
                layerModel.Properties.AnimationProgress = progress;
        }

        public void Draw(LayerPropertiesModel props, LayerPropertiesModel applied, DrawingContext c)
        {
            if (applied.Brush == null)
                return;

            const int scale = 4;
            // Set up variables for this frame
            var rect = props.Contain
                ? new Rect(applied.X*scale, applied.Y*scale, applied.Width*scale, applied.Height*scale)
                : new Rect(props.X*scale, props.Y*scale, props.Width*scale, props.Height*scale);

            var clip = new Rect(applied.X*scale, applied.Y*scale, applied.Width*scale, applied.Height*scale);

            // Can't meddle with the original brush because it's frozen.
            var brush = applied.Brush.Clone();
            brush.Opacity = (Math.Sin(props.AnimationProgress*Math.PI) + 1)*(props.Opacity/2);
            applied.Brush = brush;

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(applied.Brush, null, rect);
            c.Pop();
        }

        public bool MustExpire(LayerModel layer) => layer.Properties.AnimationProgress > 2;
    }
}