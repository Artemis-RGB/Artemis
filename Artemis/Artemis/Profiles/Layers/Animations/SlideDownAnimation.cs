using System.Windows;
using System.Windows.Media;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Animations
{
    public class SlideDownAnimation : ILayerAnimation
    {
        public string Name => "Slide down";

        public void Update(LayerModel layerModel, bool updateAnimations)
        {
            var progress = layerModel.AnimationProgress;
            if (MustExpire(layerModel))
                progress = 0;
            progress = progress + layerModel.Properties.AnimationSpeed*2;

            // If not previewing, store the animation progress in the actual model for the next frame
            if (updateAnimations)
                layerModel.AnimationProgress = progress;
        }

        public void Draw(LayerModel layerModel, DrawingContext c)
        {
            if (layerModel.Brush == null)
                return;

            // Set up variables for this frame
            var rect = layerModel.Properties.Contain
                ? layerModel.LayerRect()
                : layerModel.Properties.PropertiesRect();

            var s1 = new Rect(new Point(rect.X, rect.Y + layerModel.AnimationProgress),
                new Size(rect.Width, rect.Height));
            var s2 = new Rect(new Point(s1.X, s1.Y - rect.Height), 
                new Size(rect.Width, rect.Height + .5));

            var clip = layerModel.LayerRect();

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(layerModel.Brush, null, s1);
            c.DrawRectangle(layerModel.Brush, null, s2);
            c.Pop();
        }

        public bool MustExpire(LayerModel layer)
        {
            return layer.AnimationProgress + layer.Properties.AnimationSpeed*2 >= layer.Properties.Height*4;
        }
    }
}