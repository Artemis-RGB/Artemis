using System;
using System.Windows;
using System.Windows.Media;
using Artemis.Layers.Interfaces;
using Artemis.Models.Profiles.Layers;

namespace Artemis.Layers.Animations
{
    public class PulseAnimation : ILayerAnimation
    {
        public string Name { get; } = "Pulse";

        public void Update(LayerPropertiesModel properties, bool updateAnimations)
        {
            var progress = properties.AnimationProgress;
            if (progress > 2)
                progress = 0;
            progress = progress + properties.AnimationSpeed/2;

            // If not previewing, store the animation progress in the actual model for the next frame
            if (updateAnimations)
                properties.AnimationProgress = progress;
        }

        public void Draw(DrawingContext c, KeyboardPropertiesModel props, AppliedProperties applied)
        {
            if (applied.Brush == null)
                return;

            const int scale = 4;
            // Set up variables for this frame
            var rect = props.Contain
                ? new Rect(applied.X*scale, applied.Y*scale, applied.Width*scale, applied.Height*scale)
                : new Rect(props.X*scale, props.Y*scale, props.Width*scale, props.Height*scale);

            var clip = new Rect(applied.X*scale, applied.Y*scale, applied.Width*scale, applied.Height*scale);
            applied.Brush.Opacity = (Math.Sin(props.AnimationProgress*Math.PI) + 1)*(props.Opacity/2);

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(applied.Brush, null, rect);
            c.Pop();
        }
    }
}