using System.Windows;
using System.Windows.Media;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Animations
{
    public class SlideLeftAnimation : ILayerAnimation
    {
        public string Name => "Slide left";

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

            var s1 = new Rect(new Point(rect.X - layerModel.AnimationProgress, rect.Y),
                new Size(rect.Width + .5, rect.Height));
            var s2 = new Rect(new Point(s1.X + rect.Width, rect.Y),
                new Size(rect.Width, rect.Height));

            var clip = layerModel.LayerRect();

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(layerModel.Brush, null, s1);
            c.DrawRectangle(layerModel.Brush, null, s2);
            c.Pop();
        }

        public bool MustExpire(LayerModel layer)
        {
            return layer.AnimationProgress + layer.Properties.AnimationSpeed*2 >= layer.Properties.Width*4;
        }
    }
}