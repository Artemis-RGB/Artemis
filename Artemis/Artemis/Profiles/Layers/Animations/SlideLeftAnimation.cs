using System.Windows;
using System.Windows.Media;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Animations
{
    public class SlideLeftAnimation : ILayerAnimation
    {
        public string Name { get; } = "Slide left";

        public void Update(LayerModel layerModel, bool updateAnimations)
        {
            var progress = layerModel.Properties.AnimationProgress;
            if (MustExpire(layerModel))
                progress = 0;
            progress = progress + layerModel.Properties.AnimationSpeed*2;

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

            var s1 = new Rect(new Point(rect.X - props.AnimationProgress, rect.Y),new Size(rect.Width + .5, rect.Height));
            var s2 = new Rect(new Point(s1.X + rect.Width, rect.Y), new Size(rect.Width, rect.Height));

            var clip = new Rect(applied.X*scale, applied.Y*scale, applied.Width*scale, applied.Height*scale);

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(applied.Brush, null, s1);
            c.DrawRectangle(applied.Brush, null, s2);
            c.Pop();
        }

        public bool MustExpire(LayerModel layer)
        {
            return layer.Properties.AnimationProgress + layer.Properties.AnimationSpeed*2 >= layer.Properties.Width*4;
        }
    }
}