using System.Windows;
using System.Windows.Media;
using Artemis.Layers.Interfaces;
using Artemis.Models.Profiles.Layers;

namespace Artemis.Layers.Animations
{
    public class SlideUpAnimation : ILayerAnimation
    {
        public string Name { get; } = "Slide up";

        public void Update(LayerPropertiesModel properties, bool updateAnimations)
        {
            var progress = properties.AnimationProgress;
            if (progress + properties.AnimationSpeed*2 >= properties.Height*4)
                progress = 0;
            progress = progress + properties.AnimationSpeed*2;

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

            var s1 = new Rect(new Point(rect.X, rect.Y - props.AnimationProgress), new Size(rect.Width, rect.Height));
            var s2 = new Rect(new Point(s1.X, s1.Y + rect.Height), new Size(rect.Width, rect.Height));

            var clip = new Rect(applied.X*scale, applied.Y*scale, applied.Width*scale, applied.Height*scale);

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(applied.Brush, null, s1);
            c.DrawRectangle(applied.Brush, null, s2);
            c.Pop();
        }
    }
}